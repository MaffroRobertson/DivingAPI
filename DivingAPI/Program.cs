using DivingAPI.Data;
using DivingAPI.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication
    .CreateBuilder(args);

builder.Services.AddDbContext<DivingContext>(options =>
options.UseSqlServer((builder.Configuration.GetConnectionString("LocalDB")),sqlOptions => sqlOptions.MigrationsAssembly("DivingAPI")));  
// Or your migrations project name

var app = builder.Build();

app.MapDiveSiteEndpoints();
app.MapDiveEndpoints();
app.MapExperienceLevelEndpoints();

try
{
    await app.MigrateDbAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
    throw;
}

app.Run();
