using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Errors;

namespace TimeTracker.Api.Features.Invites;

public static class GetInviteByToken
{
    public sealed record Request(string Token);

    public sealed record Response(
        Guid Id,
        Guid WorkspaceId,
        string WorkspaceName,
        string Email,
        string Role,
        string Status,
        DateTimeOffset ExpiresAt,
        DateTimeOffset CreatedAt);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .MaximumLength(200);
        }
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/invites/{token}", Handle)
           .WithName("GetInviteByToken")
           .WithTags("Invites");
    }

    private static async Task<IResult> Handle(
        string token,
        AppDbContext db,
        IValidator<Request> validator,
        CancellationToken ct)
    {
        var request = new Request(token);
        var validation = await validator.ValidateAsync(request, ct);

        if (!validation.IsValid)
            return ApiErrors.ValidationError(validation);

        var invite = await db.WorkspaceInvites
            .Where(i => i.Token == token)
            .Join(db.Workspaces,
                i => i.WorkspaceId,
                w => w.Id,
                (i, w) => new
                {
                    Invite = i,
                    WorkspaceName = w.Name
                })
            .FirstOrDefaultAsync(ct);

        if (invite is null)
            return ApiErrors.NotFound("INVITE_NOT_FOUND", "Invite not found.");

        if (invite.Invite.Status == "pending" && invite.Invite.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            invite.Invite.Status = "expired";
            await db.SaveChangesAsync(ct);
        }

        return Results.Ok(new Response(
            invite.Invite.Id,
            invite.Invite.WorkspaceId,
            invite.WorkspaceName,
            invite.Invite.Email,
            invite.Invite.Role,
            invite.Invite.Status,
            invite.Invite.ExpiresAt,
            invite.Invite.CreatedAt));
    }
}