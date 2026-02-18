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
                new { Id = 1, Username = "testUser", PasswordHash = "$2a$11$IRw05ecQLaLiYGEvYt04yeS2KOs7VzKxB15FtapILkpekskP1LElC", RoleId = 1, RoleName = "User"},
                new { Id = 2, Username = "adminUser", PasswordHash = "$2a$11$g11E66FXSdgSav15B0MqR.uuCsEJsrCdgi9fi4dMAxBNRg2fM1V/O", RoleId = 2, RoleName = "Admin" }
            );
        }

    }
}
