using DivingAPI.Models;
using DivingAPI.Models.Auth;
using DivingAPI.Models.Lookups;
using Microsoft.EntityFrameworkCore;

namespace DivingAPI.Data
{
    public class DivingContext(DbContextOptions<DivingContext> options)
        : DbContext(options)
    {
        //Db sets for main tables
        public DbSet<Dive> Dives => Set<Dive>();
        public DbSet<DiveSite> DiveSites => Set<DiveSite>();
        public DbSet<ExperienceLevel> ExperienceLevels => Set<ExperienceLevel>();

        //Db sets for auth tables
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ExperienceLevel>().HasData(
                new  { Id = 1, Name = "Open Water" },
                new  { Id = 2, Name = "Advanced" },
                new  { Id = 3, Name = "Technical" }
            );
            modelBuilder.Entity<Role>().HasData(
                new { Id = 1, Name = "User" },
                new { Id = 2, Name = "Admin" }
            );
            modelBuilder.Entity<User>().HasData(
                new { Id = 1, Username = "testUser", PasswordHash = "testPassword", RoleId = 1, RoleName = "User"},
                new { Id = 2, Username = "adminUser", PasswordHash = "adminPassword", RoleId = 2, RoleName = "Admin" }
            );
        }

    }
}
