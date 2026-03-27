using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Errors;
using FluentValidation;
using TimeTracker.Api.Shared.Auth;

namespace TimeTracker.Api.Features.Invites;

public static class RevokeInvite
{
    public sealed record Request(Guid WorkspaceId, Guid InviteId);

    public sealed record Response(
        Guid Id,
        Guid WorkspaceId,
        string Email,
        string Role,
        string Status,
        DateTimeOffset ExpiresAt,
        DateTimeOffset CreatedAt);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.WorkspaceId)
                .NotEmpty()
                .WithMessage("WorkspaceId is required.");

            RuleFor(x => x.InviteId)
                .NotEmpty()
                .WithMessage("InviteId is required.");
        }
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/workspaces/{workspaceId:guid}/invites/{inviteId:guid}/revoke", Handle)
           .WithName("RevokeInvite")
           .WithTags("Invites");
    }

    private static async Task<IResult> Handle(
        Guid workspaceId,
        Guid inviteId,
        HttpContext http,
        AppDbContext db,
        IValidator<Request> validator,
        CancellationToken ct)
    {
        var authError = CurrentUser.TryGetUserId(http, out var userId);
        if (authError is not null) return authError;

        var request = new Request(workspaceId, inviteId);
        var validation = await validator.ValidateAsync(request, ct);

        if (!validation.IsValid)
            return ApiErrors.ValidationError(validation);

        var membership = await db.WorkspaceMembers
            .Where(x => x.WorkspaceId == workspaceId && x.UserId == userId)
            .Select(x => new { x.Role })
            .FirstOrDefaultAsync(ct);

        if (membership is null)
            return ApiErrors.NotFound("WORKSPACE_NOT_FOUND", "Workspace not found or not accessible.");

        if (membership.Role != "admin")
            return ApiErrors.Forbidden("INSUFFICIENT_PERMISSIONS", "Only admins can revoke invites.");

        var invite = await db.WorkspaceInvites
            .FirstOrDefaultAsync(x => x.Id == inviteId && x.WorkspaceId == workspaceId, ct);

        if (invite is null)
            return ApiErrors.NotFound("INVITE_NOT_FOUND", "Invite not found.");

        if (invite.Status != "pending")
            return ApiErrors.Conflict("INVITE_NOT_PENDING", "Only pending invites can be revoked.");

        if (invite.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            invite.Status = "expired";
            await db.SaveChangesAsync(ct);

            return ApiErrors.Conflict("INVITE_EXPIRED", "Invite has already expired.");
        }

        invite.Status = "revoked";

        await db.SaveChangesAsync(ct);

        return Results.Ok(new Response(
            invite.Id,
            invite.WorkspaceId,
            invite.Email,
            invite.Role,
            invite.Status,
            invite.ExpiresAt,
            invite.CreatedAt));
    }
}