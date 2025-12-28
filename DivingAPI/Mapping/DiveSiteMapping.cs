using DivingAPI.DTOs.DiveSiteDTOs;
using DivingAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace DivingAPI.Mapping
{
    public static class DiveSiteMapping
    {
        public static DiveSite ToEntity(this CreateDiveSiteDTO diveSite)
        {
            return new DiveSite()
            {
                Name = diveSite.Name,
                Location = diveSite.Location,
                ExperienceLevelId = diveSite.ExperienceLevelId,
                Description = diveSite.Description
            };
        }

        public static DiveSite ToEntity(this UpdateDiveSiteDTO diveSite, int id)
        {
            return new DiveSite()
            {
                Id = id,
                Name = diveSite.Name,
                Location = diveSite.Location,
                ExperienceLevelId = diveSite.ExperienceLevelId,
                Description = diveSite.Description
            };
        }

        public static DiveSiteSummaryDTO ToDiveSiteSummaryDTO(this DiveSite diveSite)
        {
            return new DiveSiteSummaryDTO(
                diveSite.Id,
                diveSite.Name,
                diveSite.Location,
                diveSite.ExperienceLevel!.Name
            );
        }


        public static DiveSiteDetailsDTO ToDiveSiteDetailsDTO(this DiveSite diveSite)
        {
            return new DiveSiteDetailsDTO(
                diveSite.Id,
                diveSite.Name,
                diveSite.Location,
                diveSite.ExperienceLevelId
            );
        }

    }
}
