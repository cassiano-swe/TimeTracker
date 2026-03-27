namespace TimeTracker.Api.Shared.Email;

public interface IEmailService
{
    Task SendWorkspaceInviteAsync(
        string toEmail,
        string workspaceName,
        string role,
        string inviteLink,
        CancellationToken ct = default);
}