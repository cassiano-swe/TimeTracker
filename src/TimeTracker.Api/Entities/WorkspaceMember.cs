namespace TimeTracker.Api.Entities;

public sealed class WorkspaceMember
{
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "member";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}