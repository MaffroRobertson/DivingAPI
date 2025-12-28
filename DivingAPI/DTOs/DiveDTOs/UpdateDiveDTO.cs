using System.ComponentModel.DataAnnotations;

namespace DivingAPI.DTOs.DiveDTOs
{
    public record class UpdateDiveDTO(
        [Required] int DiveSiteId,
        [Required] DateTime Date,
        [Required][Range(1, 500)] int MaxDepth,
        [Required] int Duration
    );
}
