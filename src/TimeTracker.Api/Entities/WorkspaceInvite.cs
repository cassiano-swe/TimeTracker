namespace TimeTracker.Api.Entities;

public sealed class WorkspaceInvite
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public string Email { get; set; } = default!;

    public string Role { get; set; } = "member";

    public string Token { get; set; } = default!;

    public string Status { get; set; } = "pending";
    // pending | accepted | revoked | expired

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}