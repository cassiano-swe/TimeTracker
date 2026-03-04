using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Entities;

namespace TimeTracker.Api.UseCases.Auth;

public static class GitHubAuth
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/auth/github/login", Login)
           .WithTags("Auth");

        app.MapGet("/api/v1/auth/github/callback", Callback)
           .WithTags("Auth");

        app.MapGet("/api/v1/me", Me)
           .RequireAuthorization()
           .WithTags("Auth");
    }

    private static IResult Login(IConfiguration cfg, HttpContext http)
    {
        var clientId = cfg["GitHub:ClientId"]!;
        var callbackPath = cfg["GitHub:CallbackPath"]!;

        // opcional: state simples (MVP). Em prod, use algo mais robusto + cookie/nonce.
        var state = Guid.NewGuid().ToString("N");
        http.Response.Cookies.Append("oauth_state", state, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // true em https
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(10)
        });

        var redirectUri = $"{http.Request.Scheme}://{http.Request.Host}{callbackPath}";
        var url =
            "https://github.com/login/oauth/authorize" +
            $"?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={Uri.EscapeDataString("read:user user:email")}" +
            $"&state={Uri.EscapeDataString(state)}";

        return Results.Redirect(url);
    }

    private static async Task<IResult> Callback(
    string code,
    string state,
    IConfiguration cfg,
    HttpContext http,
    IHttpClientFactory httpFactory,
    AppDbContext db,
    CancellationToken ct)
    {
        // valida state (MVP)
        if (!http.Request.Cookies.TryGetValue("oauth_state", out var expected) || expected != state)
            return Results.BadRequest(new { code = "OAUTH_STATE_INVALID", message = "Invalid OAuth state." });

        var clientId = cfg["GitHub:ClientId"]!;
        var clientSecret = cfg["GitHub:ClientSecret"]!;
        var callbackPath = cfg["GitHub:CallbackPath"]!;
        var redirectUri = $"{http.Request.Scheme}://{http.Request.Host}{callbackPath}";

        // 1) trocar code -> access_token
        var token = await ExchangeCodeForToken(httpFactory, clientId, clientSecret, code, redirectUri, ct);
        if (token is null)
            return Results.BadRequest(new { code = "OAUTH_TOKEN_FAILED", message = "Failed to get GitHub token." });

        // 2) buscar user no GitHub
        var ghUser = await FetchGitHubUser(httpFactory, token, ct);
        if (ghUser is null)
            return Results.BadRequest(new { code = "GITHUB_USER_FAILED", message = "Failed to fetch GitHub user." });

        var email = await FetchPrimaryEmail(httpFactory, token, ct) ?? ghUser.Email;
        if (string.IsNullOrWhiteSpace(email))
            return Results.BadRequest(new { code = "EMAIL_REQUIRED", message = "GitHub email not found." });

        // 3) upsert user local
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email.Trim(),
                Name = ghUser.Name ?? ghUser.Login,
                AvatarUrl = ghUser.AvatarUrl,
                GitHubUserId = ghUser.Id,
                GitHubLogin = ghUser.Login
            };
            db.Users.Add(user);
        }
        else
        {
            user.Name = ghUser.Name ?? user.Name;
            user.AvatarUrl = ghUser.AvatarUrl ?? user.AvatarUrl;
            user.GitHubUserId = ghUser.Id;
            user.GitHubLogin = ghUser.Login;
        }

        await db.SaveChangesAsync(ct);

        // 4) gerar JWT
        var jwt = CreateJwt(cfg, user);

        // MVP: devolver JSON com token (frontend salva)
        return Results.Ok(new
        {
            access_token = jwt,
            token_type = "Bearer",
            user = new { user.Id, user.Email, user.Name, user.AvatarUrl }
        });
    }

    [Authorize]
    private static async Task<IResult> Me(AppDbContext db, ClaimsPrincipal principal, CancellationToken ct)
    {
        var idStr = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        if (!Guid.TryParse(idStr, out var userId))
            return Results.Unauthorized();

        var user = await db.Users
            .Where(x => x.Id == userId)
            .Select(x => new { x.Id, x.Email, x.Name, x.AvatarUrl, x.GitHubLogin })
            .FirstOrDefaultAsync(ct);

        return user is null ? Results.Unauthorized() : Results.Ok(user);
    }

    private static async Task<string?> ExchangeCodeForToken(
            IHttpClientFactory httpFactory,
            string clientId,
            string clientSecret,
            string code,
            string redirectUri,
            CancellationToken ct)
    {
        using var client = httpFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var body = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["code"] = code,
            ["redirect_uri"] = redirectUri
        };

        req.Content = new FormUrlEncodedContent(body);

        using var resp = await client.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return null;

        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.TryGetProperty("access_token", out var t) ? t.GetString() : null;
    }

    private static async Task<GhUser?> FetchGitHubUser(IHttpClientFactory httpFactory, string token, CancellationToken ct)
    {
        var client = httpFactory.CreateClient("github");
        using var req = new HttpRequestMessage(HttpMethod.Get, "user");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var resp = await client.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return null;

        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<GhUser>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private static async Task<string?> FetchPrimaryEmail(IHttpClientFactory httpFactory, string token, CancellationToken ct)
    {
        var client = httpFactory.CreateClient("github");
        using var req = new HttpRequestMessage(HttpMethod.Get, "user/emails");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var resp = await client.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return null;

        var json = await resp.Content.ReadAsStringAsync(ct);
        var emails = JsonSerializer.Deserialize<List<GhEmail>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return emails?
            .OrderByDescending(e => e.Primary)
            .ThenByDescending(e => e.Verified)
            .FirstOrDefault(e => e.Primary && e.Verified)?.Email
            ?? emails?.FirstOrDefault(e => e.Verified)?.Email;
    }

    private static string CreateJwt(IConfiguration cfg, User user)
    {
        var jwt = cfg.GetSection("Auth:Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("github_login", user.GitHubLogin ?? "")
        };

        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record GhUser(long Id, string Login, string? Name, string? Email, string? AvatarUrl);
    private sealed record GhEmail(string Email, bool Primary, bool Verified);
}


