namespace TimeTracker.Api.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public long? GitHubUserId {get;set;}
    public string? GitHubLogin {get;set;}
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}