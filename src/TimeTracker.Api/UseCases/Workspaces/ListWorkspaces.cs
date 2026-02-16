using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Auth;

namespace TimeTracker.Api.UseCases.Workspaces;

public static class ListWorkspaces
{
    public sealed record Response(Guid Id, string Name, string Plan, string Role);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/workspaces", Handle)
           .WithName("ListWorkspaces")
           .WithTags("Workspaces");
    }

    private static async Task<IResult> Handle(HttpContext http, AppDbContext db, CancellationToken ct)
    {
        var authError = CurrentUser.TryGetUserId(http, out var userId);
        if (authError is not null) return authError;

            var data = await db.WorkspaceMembers
            .Where(m => m.UserId == userId)
            .Join(db.Workspaces,
                m => m.WorkspaceId,
                w => w.Id,
                (m, w) => new { m, w })
            .OrderByDescending(x => x.w.CreatedAt) // ou x.w.Id se não tiver CreatedAt
            .Select(x => new Response(x.w.Id, x.w.Name, x.w.Plan, x.m.Role))
            .ToListAsync(ct);

        return Results.Ok(data);
    }
}
