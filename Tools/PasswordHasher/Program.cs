using System;
using BCrypt.Net;

class Program
{
    static void Main(string[] args)
    {
        var users = new[]
        {
            new { Username = "testUser", Password = "testPassword" },
            new { Username = "adminUser", Password = "adminPassword" }
        };

        foreach (var u in users)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(u.Password);
            Console.WriteLine($"{u.Username}: {hash}");
        }
    }
}
