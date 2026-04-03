using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeTracker.Api.Shared.GitHub;

public sealed class GitHubAppClient(IHttpClientFactory httpClientFactory, GitHubAppJwtFactory jwtFactory, IConfiguration config)
{
    public async Task<string> CreateInstallationTokenAsync(long installationId, CancellationToken ct)
    {
        var baseUrl = config["GitHub:BaseUrl"] ?? "https://api.github.com";
        var jwt = jwtFactory.CreateAppJwt();

        using var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("TimeTracker");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        using var response = await client.PostAsync($"/app/installations/{installationId}/access_tokens", null, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("token").GetString()
               ?? throw new InvalidOperationException("GitHub installation token not returned.");
    }

    public async Task<GitHubInstallationRepositoriesResponse> GetInstallationRepositoriesAsync(
        string installationToken,
        CancellationToken ct)
    {
        var baseUrl = config["GitHub:BaseUrl"] ?? "https://api.github.com";

        using var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("TimeTracker");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", installationToken);
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        using var response = await client.GetAsync("/installation/repositories", ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);

        return JsonSerializer.Deserialize<GitHubInstallationRepositoriesResponse>(
                   json,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new InvalidOperationException("Failed to deserialize installation repositories.");
    }

    public sealed record GitHubInstallationRepositoriesResponse(
        [property: JsonPropertyName("total_count")] int TotalCount,
        [property: JsonPropertyName("repositories")] List<GitHubRepositoryItem> Repositories);

    public sealed record GitHubRepositoryItem(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("full_name")] string FullName,
        [property: JsonPropertyName("default_branch")] string DefaultBranch);
}