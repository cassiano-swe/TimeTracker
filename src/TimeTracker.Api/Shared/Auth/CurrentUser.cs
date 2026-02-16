using TimeTracker.Api.Shared.Errors;

namespace TimeTracker.Api.Shared.Auth;

public static class CurrentUser
{
    // MVP temporário: pega userId do header.
    // Depois, substituímos por JWT/Claims sem mudar as features.
    public static IResult? TryGetUserId(HttpContext http, out Guid userId)
    {
        userId = default;

        if (!http.Request.Headers.TryGetValue("X-User-Id", out var raw) || raw.Count == 0)
            return ApiErrors.Unauthorized("AUTH_REQUIRED", "Missing X-User-Id header.");

        if (!Guid.TryParse(raw[0], out userId))
            return ApiErrors.Unauthorized("AUTH_INVALID", "Invalid X-User-Id header.");

        return null;
    }
}
