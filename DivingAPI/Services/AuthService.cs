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

        public AuthService(DivingContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
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
            var hash = _tokenService.HashToken(refreshToken);
            var existing = await _db.RefreshTokens.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.TokenHash == hash);

            if (existing == null || !existing.IsActive)
            {
                return (false, null, null);
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                existing.Revoked = DateTime.UtcNow;
                existing.RevokedByIp = remoteIp;

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
    }
}
