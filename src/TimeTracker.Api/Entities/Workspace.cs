namespace TimeTracker.Api.Entities;

public sealed class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Plan { get; set; } = "free";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}