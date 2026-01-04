using Microsoft.AspNetCore.Builder;

namespace DivingAPI.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static WebApplication UseCustomSwagger(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    options.RoutePrefix = "swagger";
                });
            }

            return app;
        }

        public static WebApplication UseAuth(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
