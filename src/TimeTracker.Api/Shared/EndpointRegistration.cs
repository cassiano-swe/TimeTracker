using TimeTracker.Api.UseCases.Dev;
using TimeTracker.Api.UseCases.Workspaces;
using TimeTracker.Api.UseCases.Members;
using TimeTracker.Api.UseCases.Auth;
using TimeTracker.Api.Features.Invites;

namespace TimeTracker.Api.Shared;

public static class EndpointRegistration
{
    public static void MapFeatureEndpoints(this WebApplication app)
    {
        // Auth
        GitHubAuth.Map(app);

        // Workspaces
        CreateWorkspace.Map(app);
        ListWorkspaces.Map(app);
        GetWorkspace.Map(app);

        // Members
        ListMembers.Map(app);

        // Invites
        CreateInvite.Map(app);
        ListInvites.Map(app);
        RevokeInvite.Map(app);
        GetInviteByToken.Map(app);

        if (app.Environment.IsDevelopment())
        {
            SeedDevData.Map(app);
        }
    }
}