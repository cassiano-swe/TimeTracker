using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Auth;
using TimeTracker.Api.Shared.Errors;

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
           .WithName("GetWorkspace")
           .WithTags("Workspaces");
    }

    private static async Task<IResult> Handle(
        Guid workspaceId,
        HttpContext http,
        AppDbContext db,
        IValidator<Request> validator,
        CancellationToken ct)
    {
        var authError = CurrentUser.TryGetUserId(http, out var userId);
        if (authError is not null) return authError;

        var req = new Request(workspaceId);
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
            return ApiErrors.ValidationError(validation);

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
