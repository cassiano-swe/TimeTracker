using Microsoft.EntityFrameworkCore;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Entities;
using TimeTracker.Api.Shared.Errors;
using TimeTracker.Api.Shared.GitHub;

namespace TimeTracker.Api.Features.GitHub;

public static class CompleteInstallationSetup
{
    public sealed record Response(
        Guid IntegrationId,
        Guid WorkspaceId,
        long InstallationId,
        string AccountLogin,
        string AccountType,
        string Status,
        int RepositoryCount);

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/github/install/setup", Handle)
           .WithName("CompleteGitHubInstallationSetup")
           .WithTags("GitHub");
    }

    private static async Task<IResult> Handle(
    long installation_id,
    string state,
    AppDbContext db,
    GitHubAppClient gitHubClient,
    CancellationToken ct)
    {
        if (!Guid.TryParse(state, out var workspaceId))
        {
            return ApiErrors.BadRequest("WORKSPACE_CONTEXT_MISSING", "Workspace context was not found.");
        }

        var installationDetails = await gitHubClient.GetInstallationAsync(installation_id, ct);

        var installationToken = await gitHubClient.CreateInstallationTokenAsync(installation_id, ct);
        var repositoriesResponse = await gitHubClient.GetInstallationRepositoriesAsync(installationToken, ct);

        var existingIntegration = await db.GitHubIntegrations
            .FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId, ct);

        if (existingIntegration is null)
        {
            existingIntegration = new GitHubIntegration
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                InstallationId = installation_id,
                AccountLogin = installationDetails.Account.Login,
                AccountType = installationDetails.Account.Type,
                Status = "active",
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.GitHubIntegrations.Add(existingIntegration);
        }
        else
        {
            existingIntegration.InstallationId = installation_id;
            existingIntegration.AccountLogin = installationDetails.Account.Login;
            existingIntegration.AccountType = installationDetails.Account.Type;
            existingIntegration.Status = "active";
        }

        var existingRepos = await db.GitHubRepositories
            .Where(x => x.WorkspaceId == workspaceId)
            .ToListAsync(ct);

        db.GitHubRepositories.RemoveRange(existingRepos);

        foreach (var repo in repositoriesResponse.Repositories)
        {
            if (string.IsNullOrWhiteSpace(repo.Name) || string.IsNullOrWhiteSpace(repo.FullName))
                continue;

            db.GitHubRepositories.Add(new GitHubRepository
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                GitHubIntegrationId = existingIntegration.Id,
                GitHubRepositoryId = repo.Id,
                Name = repo.Name,
                FullName = repo.FullName,
                DefaultBranch = string.IsNullOrWhiteSpace(repo.DefaultBranch) ? "main" : repo.DefaultBranch,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);

        return Results.Ok(new Response(
            existingIntegration.Id,
            workspaceId,
            installation_id,
            existingIntegration.AccountLogin,
            existingIntegration.AccountType,
            existingIntegration.Status,
            repositoriesResponse.Repositories.Count));
    }
}