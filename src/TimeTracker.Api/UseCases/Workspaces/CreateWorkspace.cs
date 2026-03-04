using TimeTracker.Api.Entities;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Errors;
using FluentValidation;
using TimeTracker.Api.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TimeTracker.Api.UseCases.Workspaces;

public static class CreateWorkspace
{
    public sealed record Request(string Name);
    public sealed record Response(Guid Id, string Name, string Plan);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(80).WithMessage("Name must be at most 80 characters");
        }
    }

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/workspaces", Handle)
            .RequireAuthorization()
          .WithName("CreateWorkspace")
          .WithTags("Workspaces");
    }

    [Authorize]
    private static async Task<IResult> Handle(
        Request req, 
        AppDbContext db, 
        IValidator<Request> validator, 
        ClaimsPrincipal principal,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);

        if (!validation.IsValid)
            return ApiErrors.ValidationError(validation);

        var userId = CurrentUser.GetUserId(principal);

        var ws = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = req.Name.Trim(),
            Plan = "free"
        };

        db.Workspaces.Add(ws);

          db.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = ws.Id,
            UserId = userId,
            Role = "admin"
        });
        
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/workspaces/{ws.Id}", new Response(ws.Id, ws.Name, ws.Plan));
    }
}
