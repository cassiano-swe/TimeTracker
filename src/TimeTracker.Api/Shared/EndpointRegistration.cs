using TimeTracker.Api.UseCases.Dev;
using TimeTracker.Api.UseCases.Workspaces;

namespace TimeTracker.Api.Shared;

public static class EndpointRegistration
{
    public static void MapFeatureEndpoints(this WebApplication app)
    {
        CreateWorkspace.Map(app);
        ListWorkspaces.Map(app);
        GetWorkspace.Map(app);

        if (app.Environment.IsDevelopment())
        {
            SeedDevData.Map(app);
        }
    }
}