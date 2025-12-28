namespace DivingAPI.DTOs.DiveSiteDTOs
{
    public record class DiveSiteDetailsDTO(
        int Id,
        string Name,
        string Location,
        int ExperienceLevelId
    );
}
