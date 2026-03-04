using System.Security.Claims;
using TimeTracker.Api.Shared.Errors;

namespace TimeTracker.Api.Shared.Auth;

public static class CurrentUser
{
    public static IResult? TryGetUserId(HttpContext http, out Guid userId)
    {
        userId = default;

        if (!http.Request.Headers.TryGetValue("X-User-Id", out var raw) || raw.Count == 0)
            return ApiErrors.Unauthorized("AUTH_REQUIRED", "Missing X-User-Id header.");

        if (!Guid.TryParse(raw[0], out userId))
            return ApiErrors.Unauthorized("AUTH_INVALID", "Invalid X-User-Id header.");

        return null;
    }

    public static Guid GetUserId(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier) 
               ?? principal.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id)
            ? id
            : throw new InvalidOperationException("Missing/invalid user id claim.");
    }
}
