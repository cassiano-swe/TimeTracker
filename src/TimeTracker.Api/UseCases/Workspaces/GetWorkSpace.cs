using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Auth;
using TimeTracker.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TimeTracker.Api.UseCases.Workspaces;

public static class GetWorkspace
{
    public sealed record Request(Guid WorkspaceId);

    public sealed record Response(Guid Id, string Name, string Plan, string Role);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.WorkspaceId)
                .NotEmpty()
                .WithMessage("WorkspaceId is required.");
        }
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/workspaces/{workspaceId:guid}", Handle)
                   .RequireAuthorization()
           .WithName("GetWorkspace")
           .WithTags("Workspaces");
    }

    [Authorize]
    private static async Task<IResult> Handle(
        Guid workspaceId,
        HttpContext http,
        AppDbContext db,
        IValidator<Request> validator,
        ClaimsPrincipal principal,
        CancellationToken ct)
    {
        var userId = CurrentUser.GetUserId(principal);

        var data = await db.WorkspaceMembers
            .Where(m => m.WorkspaceId == workspaceId && m.UserId == userId)
            .Join(db.Workspaces,
                m => m.WorkspaceId,
                w => w.Id,
                (m, w) => new Response(w.Id, w.Name, w.Plan, m.Role))
            .FirstOrDefaultAsync(ct);

        return data is null
            ? ApiErrors.NotFound("WORKSPACE_NOT_FOUND", "Workspace not found or not accessible.")
            : Results.Ok(data);
    }
}
