using System.ComponentModel.DataAnnotations;

namespace DivingAPI.DTOs.DiveSiteDTOs
{
    public record class UpdateDiveSiteDTO(
        [Required] string Name,
        string Location,
        int ExperienceLevelId,
        string Description
    );
}
