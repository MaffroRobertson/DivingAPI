using Microsoft.EntityFrameworkCore;


namespace DivingAPI.Data
{
    public static class DataExtensions
    {
        public static async Task MigrateDbAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DivingContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}
