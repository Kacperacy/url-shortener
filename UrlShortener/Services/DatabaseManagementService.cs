using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Services;

public static class DatabaseManagementService
{
    public static void MigrationInitialisation(WebApplicationBuilder app)
    {
        using var serviceScope = app.Services.BuildServiceProvider().CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }
}

