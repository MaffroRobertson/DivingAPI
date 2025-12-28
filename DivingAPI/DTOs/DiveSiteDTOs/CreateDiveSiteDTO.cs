using System.ComponentModel.DataAnnotations;

namespace DivingAPI.DTOs.DiveSiteDTOs
{
    public record class CreateDiveSiteDTO(
        [Required] string Name,
        string Location,
        int ExperienceLevelId,
        string Description
    );
}
