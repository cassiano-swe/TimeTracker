namespace TimeTracker.Api.Entities;

public sealed class GitHubRepository
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public Guid GitHubIntegrationId { get; set; }

    public long GitHubRepositoryId { get; set; }

    public string Name { get; set; } = default!;

    public string FullName { get; set; } = default!;

    public string DefaultBranch { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}