using DivingAPI.DTOs;
using DivingAPI.Models.Lookups;

namespace DivingAPI.Mapping
{
    public static class ExperienceLevelMapping
    {
        public static ExperienceLevelDTO ToDto(this ExperienceLevel experienceLevel)
        {
            return new ExperienceLevelDTO(experienceLevel.Id, experienceLevel.Name);
        }
    }
}
