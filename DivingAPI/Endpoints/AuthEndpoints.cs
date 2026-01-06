using BCrypt.Net;
using DivingAPI.Data;
using DivingAPI.Models.Auth;
using DivingAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DivingAPI.Endpoints
{
    public static class AuthEndpoints
    {
        const string GetAuthEndpointName = "Login";


        public static void MapAuthEndpoints(this WebApplication app)
        {
            var authGroup = app.MapGroup("/login")
                .WithTags("Authentication")
                .AllowAnonymous();

            authGroup.MapPost("/", async (Login login, IConfiguration config, DivingContext dbContext, HttpContext http) =>
            {
                var user = await dbContext.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Username == login.Username);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                bool verified = false;

                try
                {
                    // Attempt to verify assuming stored value is a bcrypt hash
                    verified = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    // Stored password is not a bcrypt hash (likely plain text from older seed)
                    // Fall back to direct comparison and update the stored hash to bcrypt
                    if (login.Password == user.PasswordHash)
                    {
                        verified = true;
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(login.Password);
                        dbContext.Users.Update(user);
                        await dbContext.SaveChangesAsync();
                    }
                }

                if (!verified)
                {
                    return Results.Unauthorized();
                }

                // create access token
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.RoleName)
                };
                var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    Issuer = config["Jwt:Issuer"],
                    Audience = config["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // create refresh token
                var refreshToken = TokenService.CreateRefreshToken();
                var refreshHash = TokenService.HashToken(refreshToken);
                var rt = new RefreshToken
                {
                    TokenHash = refreshHash,
                    UserId = user.Id,
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(14),
                    CreatedByIp = http.Connection.RemoteIpAddress?.ToString()
                };

                dbContext.RefreshTokens.Add(rt);
                await dbContext.SaveChangesAsync();

                // set httpOnly cookie
                http.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = rt.Expires
                });

                return Results.Ok(new { Token = tokenString });
            })
            .WithName(GetAuthEndpointName)
            .WithTags("Authentication");

            // refresh endpoint
            authGroup.MapPost("/refresh", async (HttpContext http, DivingContext db) =>
            {
                string? incoming = null;
                if (http.Request.Cookies.TryGetValue("refreshToken", out var cookieVal)) incoming = cookieVal;
                if (string.IsNullOrEmpty(incoming)) return Results.Unauthorized();

                var hash = TokenService.HashToken(incoming);
                var existing = await db.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.TokenHash == hash);
                if (existing == null || !existing.IsActive)
                {
                    // possible reuse or invalid token
                    return Results.Unauthorized();
                }

                // Use an explicit transaction so revoke + insert are atomic
                await using var transaction = await db.Database.BeginTransactionAsync();
                try
                {
                    // revoke existing token and mark replacement
                    existing.Revoked = DateTime.UtcNow;
                    existing.RevokedByIp = http.Connection.RemoteIpAddress?.ToString();

                    var newToken = TokenService.CreateRefreshToken();
                    var newHash = TokenService.HashToken(newToken);
                    existing.ReplacedByTokenHash = newHash;

                    var newRt = new RefreshToken
                    {
                        TokenHash = newHash,
                        UserId = existing.UserId,
                        Created = DateTime.UtcNow,
                        Expires = DateTime.UtcNow.AddDays(14),
                        CreatedByIp = http.Connection.RemoteIpAddress?.ToString()
                    };

                    db.RefreshTokens.Add(newRt);

                    // Save both the updated existing entity and the new one
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // issue new access token
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, existing.User.Username),
                        new Claim(ClaimTypes.Role, existing.User.RoleName)
                    };
                    var key = Encoding.UTF8.GetBytes(http.RequestServices.GetRequiredService<IConfiguration>()["Jwt:Key"]!);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(claims),
                        Expires = DateTime.UtcNow.AddMinutes(15),
                        Issuer = http.RequestServices.GetRequiredService<IConfiguration>()["Jwt:Issuer"],
                        Audience = http.RequestServices.GetRequiredService<IConfiguration>()["Jwt:Audience"],
                        SigningCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(key),
                            SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    // set new refresh cookie
                    http.Response.Cookies.Append("refreshToken", newToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = newRt.Expires
                    });

                    return Results.Ok(new { Token = tokenString });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            // revoke endpoint
            authGroup.MapPost("/revoke", async (HttpContext http, DivingContext db) =>
            {
                string? incoming = null;
                if (http.Request.Cookies.TryGetValue("refreshToken", out var cookieVal)) incoming = cookieVal;
                if (string.IsNullOrEmpty(incoming)) return Results.BadRequest();

                var hash = TokenService.HashToken(incoming);
                var existing = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
                if (existing == null) return Results.NotFound();

                existing.Revoked = DateTime.UtcNow;
                existing.RevokedByIp = http.Connection.RemoteIpAddress?.ToString();
                await db.SaveChangesAsync();

                http.Response.Cookies.Delete("refreshToken");
                return Results.NoContent();
            });
        }

    }
}
