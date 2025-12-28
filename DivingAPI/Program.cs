using DivingAPI.Data;
using DivingAPI.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication
    .CreateBuilder(args);

builder.Services.AddDbContext<DivingContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DivingDB")));

var app = builder.Build();

app.MapDiveSiteEndpoints();
app.MapDiveEndpoints();
app.MapExperienceLevelEndpoints();

await app.MigrateDbAsync();

app.Run();
