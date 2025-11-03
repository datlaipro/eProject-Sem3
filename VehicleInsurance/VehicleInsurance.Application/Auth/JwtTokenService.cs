using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;


namespace VehicleInsurance.Application.Auth;

public class JwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _secret;
    private readonly TimeSpan _accessTtl;

    public JwtTokenService(string issuer, string audience, string secret, TimeSpan accessTtl)
    {
        _issuer = issuer;
        _audience = audience;
        _secret = secret;
        _accessTtl = accessTtl;
    }

    public string CreateAccessToken(AuthResult user)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
    };

        // Nếu có nhiều roles
        if (user.Roles != null)
        {
            foreach (var role in user.Roles)
            {
                // 🔥 Quan trọng: mỗi role là một claim riêng biệt
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }
        else if (!string.IsNullOrEmpty(user.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role));
        }

        // Thêm permission nếu có
        if (user.Permissions != null)
        {
            foreach (var permission in user.Permissions)
                claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(_accessTtl),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}