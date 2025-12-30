using DivingAPI.Models.Lookups;
using System.ComponentModel.DataAnnotations;

namespace DivingAPI.Models
{
    public class DiveSite
    {
        public int Id { get; set; }

        [Required] public string Name { get; set; } = string.Empty;

        [Required] public string Location { get; set; } = string.Empty;

        public int ExperienceLevelId { get; set; }

        public ExperienceLevel? ExperienceLevel { get; set; }
        
        public string? Description { get; set; } 

        public DiveSite() { }

    }
}
