using DivingAPI.Data;
using DivingAPI.DTOs.DiveDTOs;
using DivingAPI.Mapping;
using DivingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DivingAPI.Endpoints
{
    public static class DiveEndpoints
    {
        const string GetDiveEndpointName = "GetDive";

        public static RouteGroupBuilder MapDiveEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("dives")
                .WithParameterValidation();


            //GET all Dives
            group.MapGet("/", async (DivingContext dbContext) =>
                await dbContext.Dives
                .Select(d => d.ToDiveSummaryDTO())
                .ToListAsync()
                );

            //GET Dive by Id
            group.MapGet("/{id}", async (int id, DivingContext dbContext) =>
            {
                Dive? dive = await dbContext.Dives.FindAsync(id);

                return dive is null ? Results.NotFound()
                        : Results.Ok(dive.ToDiveDetailsDTO());

            }).WithName(GetDiveEndpointName);

            //POST (create)
            group.MapPost("/", async (CreateDiveDTO newDive, DivingContext dbContext) =>
            {
                Dive dive = newDive.ToEntity();

                dbContext.Dives.Add(dive);
                await dbContext.SaveChangesAsync();

                return Results.CreatedAtRoute(
                    GetDiveEndpointName,
                    new { id = dive.Id },
                    dive.ToDiveDetailsDTO());
            });

            //PUT (update)
            group.MapPut("/{id}", async (int id, UpdateDiveDTO updatedDive, DivingContext dbContext) =>
            {
                var existingDive = await dbContext.Dives.FindAsync(id);

                if (existingDive is null)
                {
                    return Results.NotFound();
                }
                dbContext.Entry(existingDive)
                    .CurrentValues
                    .SetValues(updatedDive.ToEntity(id));

                return Results.NoContent();
            });

            //DELETE Dive
            group.MapDelete("/{id}", async (int id, DivingContext dbContext) =>
            {
                await dbContext.Dives
                    .Where(d => d.Id == id)
                    .ExecuteDeleteAsync();

                return Results.NoContent();
            });

            return group;
        }
    }
}
