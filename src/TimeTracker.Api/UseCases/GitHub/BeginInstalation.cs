using TimeTracker.Api.Shared.Auth;

namespace TimeTracker.Api.Features.GitHub;

public static class BeginInstallation
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/workspaces/{workspaceId:guid}/github/install", Handle)
           .WithName("BeginGitHubInstallation")
           .WithTags("GitHub");
    }

    private static IResult Handle(
        Guid workspaceId,
        HttpContext http,
        IConfiguration config)
    {
        // var authError = CurrentUser.TryGetUserId(http, out _);
        // if (authError is not null) return authError;

        var appSlug = config["GitHubApp:AppSlug"];
        if (string.IsNullOrWhiteSpace(appSlug))
            throw new InvalidOperationException("GitHubApp:AppSlug not configured.");

        http.Response.Cookies.Append(
            "tt_github_workspace_id",
            workspaceId.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromMinutes(15)
            });

        var installUrl =
        $"https://github.com/apps/{appSlug}/installations/new" +
        $"?state={workspaceId}";

        return Results.Redirect(installUrl);
    }
}