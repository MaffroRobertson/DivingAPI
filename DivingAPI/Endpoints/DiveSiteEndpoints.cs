using DivingAPI.Data;
using DivingAPI.DTOs.DiveSiteDTOs;
using DivingAPI.Mapping;
using DivingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DivingAPI.Endpoints
{
    public static class DiveSiteEndpoints
    {
        const string GetDiveSiteEndpointName = "GetDiveSite";

        private static readonly List<DiveSiteDetailsDTO> diveSites =
            [
            new(
                1,"Sail Rock", "Thailand", 1
                ),
            new(
                2,"Osprey Reef", "Australi", 2)
            ];

        public static RouteGroupBuilder MapDiveSiteEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("divesites")
                .WithParameterValidation();

            //GET all DiveSites
            group.MapGet("/", async (DivingContext dbContext) => 
                await dbContext.DiveSites
                .Include(ds => ds.ExperienceLevel)
                .Select(ds => ds.ToDiveSiteSummaryDTO())
                .ToListAsync()
                );


            //GET DiveSite by Id
            group.MapGet("/{id}", async (int id, DivingContext dbContext) =>
            {
                DiveSite? diveSite =  await dbContext.DiveSites.FindAsync(id);

                return diveSite is null ? Results.NotFound()
                        : Results.Ok(diveSite.ToDiveSiteDetailsDTO());

            }).WithName(GetDiveSiteEndpointName);


            //POST (create)
            group.MapPost("/", async (CreateDiveSiteDTO newDiveSite, DivingContext dbContext) =>
            {
                DiveSite diveSite = newDiveSite.ToEntity();

                dbContext.DiveSites.Add(diveSite);
                await dbContext.SaveChangesAsync();

                return Results.CreatedAtRoute(
                    GetDiveSiteEndpointName,
                    new { id = diveSite.Id },
                    diveSite.ToDiveSiteDetailsDTO());
            });

            //PUT (update)
            group.MapPut("/{id}", async (int id, UpdateDiveSiteDTO updatedDiveSite, DivingContext dbContext) =>
            {
                var existingDiveSite = await dbContext.DiveSites.FindAsync(id);

                if(existingDiveSite is null)
                {
                    return Results.NotFound();
                }
                dbContext.Entry(existingDiveSite)
                    .CurrentValues
                    .SetValues(updatedDiveSite.ToEntity(id));

                return Results.NoContent();
            });

            //DELETE DiveSite
            group.MapDelete("/{id}", async (int id, DivingContext dbContext) =>
            {
                await dbContext.DiveSites
                    .Where(ds => ds.Id == id)
                    .ExecuteDeleteAsync();

                return Results.NoContent();
            });

            return group;
        }
    }
}
