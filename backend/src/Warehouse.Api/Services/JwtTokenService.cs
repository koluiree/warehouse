using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Warehouse.Api.Services;

public class JwtTokenService(IConfiguration configuration)
{
    public string CreateToken(string userId, string username, IEnumerable<string> roles)
    {
        var jwt = configuration.GetSection("Jwt");
        var key = jwt["Key"] ?? "dev-super-secret-key-change-me";
        var issuer = jwt["Issuer"] ?? "warehouse-api";
        var audience = jwt["Audience"] ?? "warehouse-client";
        var expiryHours = int.TryParse(jwt["ExpiryHours"], out var h) ? h : 8;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
