namespace TimeTracker.Api.Entities;

public sealed class GitHubIntegration
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public long InstallationId { get; set; }

    public string AccountLogin { get; set; } = default!;

    public string AccountType { get; set; } = default!;
    // User | Organization

    public string Status { get; set; } = "active";
    // active | disconnected

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}