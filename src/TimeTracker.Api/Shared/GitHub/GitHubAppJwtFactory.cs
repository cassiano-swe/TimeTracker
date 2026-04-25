using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace TimeTracker.Api.Shared.GitHub;

public sealed class GitHubAppJwtFactory(IConfiguration config, IWebHostEnvironment env)
{
    public string CreateAppJwt()
    {
        var appId = config["GitHubApp:AppId"]
            ?? throw new InvalidOperationException("GitHubApp:AppId not configured.");

        var configuredPath = config["GitHubApp:PrivateKeyPath"]
            ?? throw new InvalidOperationException("GitHub:PrivateKeyPath not configured.");

        var privateKeyPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(env.ContentRootPath, configuredPath);

        if (!File.Exists(privateKeyPath))
            throw new InvalidOperationException($"GitHubApp private key file not found: {privateKeyPath}");

        var pem = File.ReadAllText(privateKeyPath);

        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);

        var securityKey = new RsaSecurityKey(rsa)
        {
            KeyId = appId
        };

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.RsaSha256);

        var now = DateTimeOffset.UtcNow;

        var token = new JwtSecurityToken(
            issuer: appId,
            notBefore: now.AddSeconds(-30).UtcDateTime,
            expires: now.AddMinutes(9).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}