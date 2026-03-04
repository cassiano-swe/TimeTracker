using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TimeTracker.Api.UseCases.Workspaces;

public static class ListWorkspaces
{
    public sealed record Response(Guid Id, string Name, string Plan, string Role);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/workspaces", Handle)
                   .RequireAuthorization()
           .WithName("ListWorkspaces")
           .WithTags("Workspaces");
    }

    [Authorize]
    private static async Task<IResult> Handle(AppDbContext db, ClaimsPrincipal principal, CancellationToken ct)
    {
        var userId = CurrentUser.GetUserId(principal);

            var data = await db.WorkspaceMembers
            .Where(m => m.UserId == userId)
            .Join(db.Workspaces,
                m => m.WorkspaceId,
                w => w.Id,
                (m, w) => new { m, w })
            .OrderByDescending(x => x.w.CreatedAt) 
            .Select(x => new Response(x.w.Id, x.w.Name, x.w.Plan, x.m.Role))
            .ToListAsync(ct);

        return Results.Ok(data);
    }
}
