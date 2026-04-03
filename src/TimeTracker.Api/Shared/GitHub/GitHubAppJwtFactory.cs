using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        using var rsa = RSA.Create();
        rsa.ImportFromPem(pem);

        var credentials = new SigningCredentials(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha256);

        var now = DateTimeOffset.UtcNow;

        var token = new JwtSecurityToken(
            issuer: appId,
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Exp, now.AddMinutes(9).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Iss, appId)
            },
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}