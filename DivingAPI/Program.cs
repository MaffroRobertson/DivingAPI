using DivingAPI.Data;
using DivingAPI.Endpoints;
using DivingAPI.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication
    .CreateBuilder(args);

// configure services via extensions
builder.Services.AddEndpointsApiExplorer();

//configure database and migrations
builder.Services.AddDatabase(builder.Configuration);

// Authorization and Authentication with JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

//configure swagger
builder.Services.AddCustomSwagger();


var app = builder.Build();

// Ensure authentication/authorization middleware runs before endpoint handling
app.UseAuthentication();
app.UseAuthorization();

//map endpoints
app.MapAuthEndpoints();
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

//setup swagger for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        // Optional: nicer default page
        options.RoutePrefix = "swagger";  // Makes /swagger the root
    });
}

app.Run();
