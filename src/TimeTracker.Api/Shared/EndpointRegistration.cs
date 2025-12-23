using TimeTracker.Api.UseCases.Workspaces;

namespace TimeTracker.Api.Shared;

public static class EndpointRegistration
{
    public static void MapFeatureEndpoints(this WebApplication app)
    {
        CreateWorkspace.Map(app);
        ListWorkspaces.Map(app);
    }
}