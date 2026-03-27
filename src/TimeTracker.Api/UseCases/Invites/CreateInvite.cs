using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Api.Shared.Email;
using TimeTracker.Api.Entities;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Errors;
using FluentValidation;
using TimeTracker.Api.Shared.Auth;

namespace TimeTracker.Api.Features.Invites;

public static class CreateInvite
{
    public sealed record Request(string Email, string Role);

    public sealed record Response(
        Guid Id,
        Guid WorkspaceId,
        string Email,
        string Role,
        string Status,
        string Token,
        DateTimeOffset ExpiresAt,
        DateTimeOffset CreatedAt);

    public sealed class Validator : AbstractValidator<Request>
    {
        private static readonly string[] AllowedRoles = ["admin", "member"];

        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            RuleFor(x => x.Role)
                .NotEmpty()
                .Must(role => AllowedRoles.Contains(role.Trim().ToLowerInvariant()))
                .WithMessage("Role must be either 'admin' or 'member'.");
        }
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/workspaces/{workspaceId:guid}/invites", Handle)
           .WithName("CreateInvite")
           .WithTags("Invites");
    }

    private static async Task<IResult> Handle(
        Guid workspaceId,
        Request request,
        HttpContext http,
        AppDbContext db,
        IValidator<Request> validator,
        [FromServices]IEmailService emailService,
        //TODO: REFATORAR
        [FromServices] IConfiguration config,
        CancellationToken ct)
    {
        var authError = CurrentUser.TryGetUserId(http, out var userId);
        if (authError is not null) return authError;

        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ApiErrors.ValidationError(validation);

        var normalizedRole = request.Role.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var workspaceAccess = await db.WorkspaceMembers
    .Where(x => x.WorkspaceId == workspaceId && x.UserId == userId)
    .Join(
        db.Workspaces,
        m => m.WorkspaceId,
        w => w.Id,
        (m, w) => new
        {
            m.Role,
            WorkspaceName = w.Name
        })
    .FirstOrDefaultAsync(ct);

        if (workspaceAccess is null)
            return ApiErrors.NotFound("WORKSPACE_NOT_FOUND", "Workspace not found or not accessible.");

        if (workspaceAccess.Role != "admin")
            return ApiErrors.Forbidden("INSUFFICIENT_PERMISSIONS", "Only admins can invite members.");

        var isAlreadyMember = await db.WorkspaceMembers
            .Where(m => m.WorkspaceId == workspaceId)
            .Join(db.Users,
                m => m.UserId,
                u => u.Id,
                (m, u) => new { u.Email })
            .AnyAsync(x => x.Email.ToLower() == normalizedEmail, ct);

        if (isAlreadyMember)
            return ApiErrors.Conflict("MEMBER_ALREADY_EXISTS", "This email is already a member of the workspace.");

        var existingPendingInvite = await db.WorkspaceInvites
            .Where(x => x.WorkspaceId == workspaceId
                     && x.Email.ToLower() == normalizedEmail
                     && x.Status == "pending"
                     && x.ExpiresAt > DateTimeOffset.UtcNow)
            .FirstOrDefaultAsync(ct);

        if (existingPendingInvite is not null)
            return ApiErrors.Conflict("INVITE_ALREADY_EXISTS", "There is already a pending invite for this email.");

        var invite = new WorkspaceInvite
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Email = normalizedEmail,
            Role = normalizedRole,
            Token = Guid.NewGuid().ToString("N"),
            Status = "pending",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.WorkspaceInvites.Add(invite);
        await db.SaveChangesAsync(ct);

        //TODO: REFATORAR
        var frontendBaseUrl = config["App:FrontendBaseUrl"]?.TrimEnd('/')
                     ?? "http://localhost:5173";

        var inviteLink = $"{frontendBaseUrl}/invite/{invite.Token}";

        await emailService.SendWorkspaceInviteAsync(
            invite.Email,
            workspaceAccess.WorkspaceName,
            invite.Role,
            inviteLink,
            ct);

        return Results.Created(
            $"/api/v1/workspaces/{workspaceId}/invites/{invite.Id}",
            new Response(
                invite.Id,
                invite.WorkspaceId,
                invite.Email,
                invite.Role,
                invite.Status,
                invite.Token,
                invite.ExpiresAt,
                invite.CreatedAt));
    }
}