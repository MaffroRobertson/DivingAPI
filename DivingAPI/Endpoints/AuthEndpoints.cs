using DivingAPI.Models.Auth;
using DivingAPI.Services;

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

            authGroup.MapPost("/", async (Login login, HttpContext http, IAuthService authService) =>
            {
                var remoteIp = http.Connection.RemoteIpAddress?.ToString();
                var (success, combined) = await authService.LoginAsync(login, remoteIp);
                if (!success || combined == null)
                {
                    return Results.Unauthorized();
                }

                var parts = combined.Split('|');
                var accessToken = parts[0];
                var refreshToken = parts[1];

                http.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(14)
                });

                return Results.Ok(new { Token = accessToken });
            })
            .WithName(GetAuthEndpointName)
            .WithTags("Authentication");

            authGroup.MapPost("/refresh", async (HttpContext http, IAuthService authService) =>
            {
                if (!http.Request.Cookies.TryGetValue("refreshToken", out var incoming) ||
                    string.IsNullOrEmpty(incoming))
                {
                    return Results.Unauthorized();
                }

                var remoteIp = http.Connection.RemoteIpAddress?.ToString();
                var (success, newAccessToken, newRefreshToken) = await authService.RefreshAsync(incoming, remoteIp);
                if (!success || newAccessToken == null || newRefreshToken == null)
                {
                    return Results.Unauthorized();
                }

                http.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(14)
                });

                return Results.Ok(new { Token = newAccessToken });
            });

            authGroup.MapPost("/revoke", async (HttpContext http, IAuthService authService) =>
            {
                if (!http.Request.Cookies.TryGetValue("refreshToken", out var incoming) ||
                    string.IsNullOrEmpty(incoming))
                {
                    return Results.BadRequest();
                }

                var remoteIp = http.Connection.RemoteIpAddress?.ToString();
                var revoked = await authService.RevokeAsync(incoming, remoteIp);

                if (!revoked)
                {
                    return Results.NotFound();
                }

                http.Response.Cookies.Delete("refreshToken");
                return Results.NoContent();
            });
        }
    }
}
