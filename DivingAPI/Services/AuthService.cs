using BCrypt.Net;
using DivingAPI.Data;
using DivingAPI.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace DivingAPI.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? AccessToken)> LoginAsync(Login login, string? remoteIp);
        Task<(bool Success, string? NewAccessToken, string? NewRefreshToken)> RefreshAsync(string refreshToken, string? remoteIp);
        Task<bool> RevokeAsync(string refreshToken, string? remoteIp);
    }

    public class AuthService : IAuthService
    {
        private readonly DivingContext _db;
        private readonly ITokenService _tokenService;
        private readonly int _maxActiveRefreshTokensPerUser;
        private readonly ILogger<AuthService> _logger;

        public AuthService(DivingContext db, ITokenService tokenService, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _db = db;
            _tokenService = tokenService;
            _maxActiveRefreshTokensPerUser = configuration.GetValue<int?>("Auth:RefreshTokens:MaxActivePerUser") ?? 5;
            if (_maxActiveRefreshTokensPerUser <= 0) _maxActiveRefreshTokensPerUser = 5;
            _logger = logger;
        }

        public async Task<(bool Success, string? AccessToken)> LoginAsync(Login login, string? remoteIp)
        {
            var user = await _db.Users.Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Username == login.Username);

            if (user == null)
            {
                return (false, null);
            }

            var verified = false;

            try
            {
                verified = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);
            }
            catch (SaltParseException)
            {
                if (login.Password == user.PasswordHash)
                {
                    verified = true;
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(login.Password);
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync();
                }
            }

            if (!verified)
            {
                return (false, null);
            }

            // Opportunistic cleanup
            await PurgeExpiredRefreshTokensAsync();

            // Trim existing active tokens so that after adding one we still respect the limit.
            await EnforceActiveRefreshTokenLimitAsync(user.Id, keepNewestCount: Math.Max(0, _maxActiveRefreshTokensPerUser - 1), remoteIp);

            var accessToken = _tokenService.CreateAccessToken(user);

            var refreshToken = _tokenService.CreateRefreshToken();
            var refreshHash = _tokenService.HashToken(refreshToken);

            var rt = new RefreshToken
            {
                TokenHash = refreshHash,
                UserId = user.Id,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(14),
                CreatedByIp = remoteIp
            };

            _db.RefreshTokens.Add(rt);
            await _db.SaveChangesAsync();

            // Return refresh token to caller so endpoint can handle cookie
            return (true, accessToken + "|" + refreshToken);
        }

        public async Task<(bool Success, string? NewAccessToken, string? NewRefreshToken)> RefreshAsync(string refreshToken, string? remoteIp)
        {
            // Opportunistic cleanup
            await PurgeExpiredRefreshTokensAsync();

            var hash = _tokenService.HashToken(refreshToken);
            var existing = await _db.RefreshTokens.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.TokenHash == hash);

            if (existing == null)
            {
                return (false, null, null);
            }

            // Reuse detection: token exists but isn't active (revoked/replaced/expired).
            // If this token was rotated (ReplacedByTokenHash set) and later presented again,
            // assume theft/replay and revoke all refresh tokens for that user.
            if (!existing.IsActive)
            {
                if (!string.IsNullOrWhiteSpace(existing.ReplacedByTokenHash))
                {
                    _logger.LogWarning(
                        "Refresh token reuse detected. UserId={UserId} TokenId={TokenId} RemoteIp={RemoteIp}",
                        existing.UserId,
                        existing.Id,
                        remoteIp);

                    await RevokeAllUserRefreshTokensAsync(existing.UserId, remoteIp);
                }

                return (false, null, null);
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                existing.Revoked = DateTime.UtcNow;
                existing.RevokedByIp = remoteIp;

                // Ensure we don't exceed the active-token limit once the new token is added.
                await EnforceActiveRefreshTokenLimitAsync(existing.UserId, keepNewestCount: Math.Max(0, _maxActiveRefreshTokensPerUser - 1), remoteIp);

                var newToken = _tokenService.CreateRefreshToken();
                var newHash = _tokenService.HashToken(newToken);
                existing.ReplacedByTokenHash = newHash;

                var newRt = new RefreshToken
                {
                    TokenHash = newHash,
                    UserId = existing.UserId,
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(14),
                    CreatedByIp = remoteIp
                };

                _db.RefreshTokens.Add(newRt);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                var accessToken = _tokenService.CreateAccessToken(existing.User);

                return (true, accessToken, newToken);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RevokeAsync(string refreshToken, string? remoteIp)
        {
            var hash = _tokenService.HashToken(refreshToken);
            var existing = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
            if (existing == null)
            {
                return false;
            }

            existing.Revoked = DateTime.UtcNow;
            existing.RevokedByIp = remoteIp;
            await _db.SaveChangesAsync();

            return true;
        }

        private Task PurgeExpiredRefreshTokensAsync()
        {
            var now = DateTime.UtcNow;
            return _db.RefreshTokens
                .Where(rt => rt.Expires <= now)
                .ExecuteDeleteAsync();
        }

        private async Task EnforceActiveRefreshTokenLimitAsync(int userId, int keepNewestCount, string? remoteIp)
        {
            // We revoke (not delete) active tokens beyond the newest N.
            // Ordering by Created desc ensures the most recent sessions survive.
            var active = await _db.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
                .OrderByDescending(rt => rt.Created)
                .ToListAsync();

            var excess = active.Skip(keepNewestCount).ToList();
            if (excess.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            foreach (var rt in excess)
            {
                rt.Revoked = now;
                rt.RevokedByIp = remoteIp;
            }

            await _db.SaveChangesAsync();
        }

        private async Task RevokeAllUserRefreshTokensAsync(int userId, string? remoteIp)
        {
            var now = DateTime.UtcNow;

            // Revoke all non-expired, non-revoked tokens.
            var tokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.Revoked == null && rt.Expires > now)
                .ToListAsync();

            foreach (var rt in tokens)
            {
                rt.Revoked = now;
                rt.RevokedByIp = remoteIp;
            }

            if (tokens.Count > 0)
            {
                await _db.SaveChangesAsync();
            }
        }
    }
}
