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
        // DÙNG ClaimTypes để tương thích rộng rãi
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role ?? "CUSTOMER")
        };

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,              // <- IEnumerable<Claim>, KHÔNG phải string
            notBefore: now,
            expires: now.Add(_accessTtl),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
