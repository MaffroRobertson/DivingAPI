namespace DivingAPI.DTOs.DiveSiteDTOs
{
    public record class DiveSiteSummaryDTO(
        int Id,
        string Name,
        string Location,
        string ExperienceLevel,
        string Description
    );
}
