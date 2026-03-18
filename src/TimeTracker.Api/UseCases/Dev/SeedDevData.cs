using TimeTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Infrastructure.Persistence;

namespace TimeTracker.Api.UseCases.Dev;

public static class SeedDevData
{
    public sealed record Response(
        Guid UserId,
        string Email,
        List<WorkspaceInfo> Workspaces);

    public sealed record WorkspaceInfo(
        Guid WorkspaceId,
        string Name,
        string Role);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/dev/seed", Handle)
           .WithName("DevSeed")
           .WithTags("Dev");
    }

    private static async Task<IResult> Handle(AppDbContext db, CancellationToken ct)
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        const string email = "admin@timetracker.dev";

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null)
        {
            user = new User
            {
                Id = userId,
                Email = email,
                Name = "TimeTracker Admin"
            };

            db.Users.Add(user);
        }

        var ws1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var ws2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var ws1 = await db.Workspaces.FirstOrDefaultAsync(x => x.Id == ws1Id, ct);
        if (ws1 is null)
        {
            ws1 = new Workspace
            {
                Id = ws1Id,
                Name = "TimeTracker Dev Workspace (Admin)",
                Plan = "free"
            };
            db.Workspaces.Add(ws1);
        }

        var ws2 = await db.Workspaces.FirstOrDefaultAsync(x => x.Id == ws2Id, ct);
        if (ws2 is null)
        {
            ws2 = new Workspace
            {
                Id = ws2Id,
                Name = "TimeTracker Dev Workspace (Member)",
                Plan = "free"
            };
            db.Workspaces.Add(ws2);
        }

        var m1 = await db.WorkspaceMembers
            .FirstOrDefaultAsync(x => x.WorkspaceId == ws1Id && x.UserId == userId, ct);

        if (m1 is null)
        {
            db.WorkspaceMembers.Add(new WorkspaceMember
            {
                WorkspaceId = ws1Id,
                UserId = userId,
                Role = "admin"
            });
        }

        var m2 = await db.WorkspaceMembers
            .FirstOrDefaultAsync(x => x.WorkspaceId == ws2Id && x.UserId == userId, ct);

        if (m2 is null)
        {
            db.WorkspaceMembers.Add(new WorkspaceMember
            {
                WorkspaceId = ws2Id,
                UserId = userId,
                Role = "member"
            });
        }

        await db.SaveChangesAsync(ct);

        return Results.Ok(new Response(
            userId,
            email,
            new List<WorkspaceInfo>
            {
                new(ws1Id, ws1.Name, "admin"),
                new(ws2Id, ws2.Name, "member")
            }
        ));
    }
}