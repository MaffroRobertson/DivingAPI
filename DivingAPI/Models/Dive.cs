using DivingAPI.Models.Lookups;
using System.ComponentModel.DataAnnotations;

namespace DivingAPI.Models
{
    public class Dive
    {
        public int Id { get; set; }
        [Required] public int DiveSiteId { get; set; }
        public DiveSite? DiveSite { get; set; }
        [Required] public DateTime Date { get; set; }
        [Required] public int Duration { get; set; } // in minutes
        [Required][Range(1, 500)] public int MaxDepth { get; set; } // in meters
        public string Notes { get; set; }

        public Dive() { }
    }
}
