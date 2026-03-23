using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Errors;
using FluentValidation;
using TimeTracker.Api.Shared.Auth;

namespace TimeTracker.Api.Features.Invites;

public static class ListInvites
{
    public sealed record Response(
        Guid Id,
        string Email,
        string Role,
        string Status,
        DateTimeOffset ExpiresAt,
        DateTimeOffset CreatedAt);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/workspaces/{workspaceId:guid}/invites", Handle)
           .WithName("ListInvites")
           .WithTags("Invites");
    }

    private static async Task<IResult> Handle(
        Guid workspaceId,
        HttpContext http,
        AppDbContext db,
        CancellationToken ct)
    {
        var authError = CurrentUser.TryGetUserId(http, out var userId);
        if (authError is not null) return authError;

        var membership = await db.WorkspaceMembers
            .Where(x => x.WorkspaceId == workspaceId && x.UserId == userId)
            .Select(x => new { x.Role })
            .FirstOrDefaultAsync(ct);

        if (membership is null)
            return ApiErrors.NotFound("WORKSPACE_NOT_FOUND", "Workspace not found or not accessible.");

        if (membership.Role != "admin")
            return ApiErrors.Forbidden("INSUFFICIENT_PERMISSIONS", "Only admins can view invites.");

        var invites = await db.WorkspaceInvites
            .Where(x => x.WorkspaceId == workspaceId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new Response(
                x.Id,
                x.Email,
                x.Role,
                x.Status,
                x.ExpiresAt,
                x.CreatedAt))
            .ToListAsync(ct);

        return Results.Ok(invites);
    }
}