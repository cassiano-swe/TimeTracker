using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Auth;
using TimeTracker.Api.Shared.Errors;
using System.Security.Claims;

namespace TimeTracker.Api.UseCases.Members;

public static class ListMembers
{
    public sealed record Response(Guid UserId, string Email, string? Name, string Role);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/workspaces/{workspaceId:guid}/members", Handle)
           .RequireAuthorization()
           .WithTags("Members");
    }

    [Authorize]
    private static async Task<IResult> Handle(Guid workspaceId, AppDbContext db, ClaimsPrincipal principal, CancellationToken ct)
    {
        var userId = CurrentUser.GetUserId(principal);

        var canAccess = await db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId, ct);
        if (!canAccess)
            return ApiErrors.NotFound("WORKSPACE_NOT_FOUND", "Workspace not found or not accessible.");

        var data = await db.WorkspaceMembers
            .Where(m => m.WorkspaceId == workspaceId)
            .Join(
                db.Users,
                m => m.UserId,
                u => u.Id,
                (m, u) => new { m, u }
            )
            .OrderBy(x => x.u.Email)
            .Select(x => new Response(
                x.u.Id,
                x.u.Email,
                x.u.Name,
                x.m.Role
            ))
            .ToListAsync(ct);

        return Results.Ok(data);
    }
}