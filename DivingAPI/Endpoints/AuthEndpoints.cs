using DivingAPI.Data;
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

            app.MapPost("/login", async (Models.Login login, IConfiguration config) =>
            {
                if(login.Username != "testUser" || login.Password != "testPassword")
                {
                    return Results.Unauthorized();
                }
                var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, login.Username),
                        new Claim(ClaimTypes.Role, "User")
                    }),
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
