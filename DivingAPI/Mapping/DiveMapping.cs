using DivingAPI.DTOs.DiveDTOs;
using DivingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DivingAPI.Mapping
{
    public static class DiveMapping
    {
        public static Dive ToEntity(this CreateDiveDTO dive)
        {
            return new Dive()
            {
                DiveSiteId = dive.DiveSiteId,
                Date = dive.Date,
                Duration = dive.Duration,
                MaxDepth = dive.MaxDepth
            };
        }

        public static Dive ToEntity(this UpdateDiveDTO dive, int id)
        {
            return new Dive()
            {
                Id = id,
                DiveSiteId = dive.DiveSiteId,
                Date = dive.Date,
                Duration = dive.Duration,
                MaxDepth = dive.MaxDepth
            };
        }
        public static DiveSummaryDTO ToDiveSummaryDTO(this Dive dive)
        {
            return new DiveSummaryDTO(
                dive.Id,
                dive.DiveSite!.Name,
                dive.Date,
                dive.Duration,
                dive.MaxDepth
            );
        }

        public static DiveDetailsDTO ToDiveDetailsDTO(this Dive dive)
        {
            return new DiveDetailsDTO(
                dive.Id,
                dive.DiveSiteId,
                dive.Date,
                dive.Duration,
                dive.MaxDepth
            );
        }
    }
}
