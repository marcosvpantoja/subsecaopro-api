using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Api.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateLawyerToken(Lawyer lawyer)
    {
        return GenerateToken(
            lawyer.Id,
            lawyer.Name,
            lawyer.OabNumber,
            "lawyer",
            lawyer.SubsectionId,
            ["lawyer"]);
    }

    public string GenerateBackofficeToken(AdminUser admin)
    {
        return GenerateToken(
            admin.Id,
            admin.Name,
            admin.Email,
            "backoffice",
            admin.SubsectionId,
            [admin.Role]);
    }

    public string GeneratePlatformAdminToken(PlatformAdmin admin)
    {
        return GenerateToken(
            admin.Id,
            admin.Name,
            admin.Email,
            "platform_admin",
            null,
            ["platform_admin"]);
    }

    private string GenerateToken(
        Guid subjectId,
        string name,
        string loginIdentifier,
        string accountType,
        Guid? subsectionId,
        string[] roles)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, subjectId.ToString()),
            new(ClaimTypes.Name, name),
            new("login", loginIdentifier),
            new("account_type", accountType),
        };

        if (subsectionId.HasValue)
            claims.Add(new Claim("subsection_id", subsectionId.Value.ToString()));

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
