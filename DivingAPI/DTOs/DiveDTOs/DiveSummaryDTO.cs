namespace DivingAPI.DTOs.DiveDTOs
{
    public record class DiveSummaryDTO(
        int Id,
        string DiveSite,
        DateTime Date,
        int MaxDepth,
        int Duration
    );
}
