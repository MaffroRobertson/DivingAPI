using BCrypt.Net;
using DivingAPI.Data;
using DivingAPI.Models.Auth;
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

            app.MapPost("/login", async (Login login, IConfiguration config, DivingContext dbContext) =>
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == login.Username);
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

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.RoleName)
                };
                var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = config["Jwt:Issuer"],
                    Audience = config["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Results.Ok(new { Token = tokenString });
            })
            .WithName(GetAuthEndpointName)
            .WithTags("Authentication");
        }

    }
}
