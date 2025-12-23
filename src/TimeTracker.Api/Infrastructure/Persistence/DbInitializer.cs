using Microsoft.EntityFrameworkCore;

namespace TimeTracker.Api.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();
    }
}
