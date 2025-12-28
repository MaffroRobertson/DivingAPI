namespace DivingAPI.DTOs.DiveDTOs
{
    public record class DiveDetailsDTO(
        int Id,
        int DiveSiteId,
        DateTime Date,
        int MaxDepth,
        int Duration
    );

}
