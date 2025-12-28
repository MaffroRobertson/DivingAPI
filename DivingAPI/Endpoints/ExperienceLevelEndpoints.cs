using DivingAPI.Data;
using DivingAPI.Mapping;
using Microsoft.EntityFrameworkCore;


namespace DivingAPI.Endpoints
{
    public static class ExperienceLevelEndpoints
    {
        public static RouteGroupBuilder MapExperienceLevelEndpoints(this WebApplication app)
        { 
            var group = app.MapGroup("experiencelevels")
                .WithParameterValidation();

            group.MapGet("/", async (DivingContext dbContext) =>
            await dbContext.ExperienceLevels
                            .Select(level => level.ToDto())
                            .AsNoTracking()
                            .ToListAsync());
            return group;
        
                }
    }
}
