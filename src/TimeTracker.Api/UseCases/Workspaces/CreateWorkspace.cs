using TimeTracker.Api.Entities;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared.Errors;
using FluentValidation;

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
          .WithName("CreateWorkspace")
          .WithTags("Workspaces");
    }

    private static async Task<IResult> Handle(Request req, AppDbContext db, IValidator<Request> validator, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);

        if (!validation.IsValid)
            return ApiErrors.ValidationError(validation);

        var ws = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = req.Name.Trim(),
            Plan = "free"
        };

        db.Workspaces.Add(ws);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/workspaces/{ws.Id}", new Response(ws.Id, ws.Name, ws.Plan));
    }
}
