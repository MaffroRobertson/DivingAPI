using DivingAPI.Data;
using DivingAPI.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;

var builder = WebApplication
    .CreateBuilder(args);

//configure swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    const string securitySchemeId = "Bearer";

    // Add this block for JWT Bearer support
    options.AddSecurityDefinition(securitySchemeId, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

//configure database and migrations
builder.Services.AddDbContext<DivingContext>(options =>
options.UseSqlServer((builder.Configuration.GetConnectionString("LocalDB")),sqlOptions => sqlOptions.MigrationsAssembly("DivingAPI")));

// Authorization and Authentication with JWT
var JwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(JwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = JwtSettings["Issuer"],
        ValidAudience = JwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
builder.Services.AddAuthorization();

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
