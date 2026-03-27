using Microsoft.Extensions.Logging;

namespace TimeTracker.Api.Shared.Email;

public sealed class ConsoleEmailService(ILogger<ConsoleEmailService> logger) : IEmailService
{
    public Task SendWorkspaceInviteAsync(
        string toEmail,
        string workspaceName,
        string role,
        string inviteLink,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            """
            ================== WORKSPACE INVITE EMAIL ==================
            To: {ToEmail}
            Workspace: {WorkspaceName}
            Role: {Role}
            InviteLink: {InviteLink}
            ===========================================================
            """,
            toEmail,
            workspaceName,
            role,
            inviteLink);

        return Task.CompletedTask;
    }
}