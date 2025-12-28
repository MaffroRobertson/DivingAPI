using DivingAPI.Models;
using DivingAPI.Models.Lookups;
using Microsoft.EntityFrameworkCore;

namespace DivingAPI.Data
{
    public class DivingContext(DbContextOptions<DivingContext> options)
        : DbContext(options)
    {
        public DbSet<Dive> Dives => Set<Dive>();
        public DbSet<DiveSite> DiveSites => Set<DiveSite>();
        public DbSet<ExperienceLevel> ExperienceLevels => Set<ExperienceLevel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ExperienceLevel>().HasData(
                new  { Id = 1, Name = "Open Water" },
                new  { Id = 2, Name = "Advanced" },
                new  { Id = 3, Name = "Technical" }
            );
        }
    }
}
