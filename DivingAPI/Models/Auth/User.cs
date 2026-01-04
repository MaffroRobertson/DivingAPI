using System.Security.Claims;

namespace DivingAPI.Models.Auth
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public List<RefreshToken> RefreshTokens { get; set; } = new();
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "User";
        public Role? Role { get; set; }
    }
}
