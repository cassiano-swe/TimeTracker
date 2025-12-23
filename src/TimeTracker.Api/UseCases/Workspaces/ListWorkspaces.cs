using TimeTracker.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TimeTracker.Api.UseCases.Workspaces;

public static class ListWorkspaces
{
    public sealed record Response(Guid Id, string Name, string Plan);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/workspaces", async (AppDbContext db) =>
        {
            var data = await db.Workspaces
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new Response(x.Id, x.Name, x.Plan))
                .ToListAsync();

            return Results.Ok(data);
        })
        .WithName("ListWorkspaces");
    }
}
