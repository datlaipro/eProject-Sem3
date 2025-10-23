using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using VehicleInsurance.Domain.Users;
using VehicleInsurance.Domain.Auth;
using VehicleInsurance.Application.Common.Errors;
using VehicleInsurance.Application.Common.Exceptions;
namespace VehicleInsurance.Application.Auth;


public class AuthService
{
    private readonly IUserRepository _users;
  
    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly JwtTokenService _jwt;
    private readonly TimeSpan _refreshTtl = TimeSpan.FromDays(30);
    private readonly ILogger<AuthService> _log;
    public AuthService(IUserRepository users, IRefreshTokenRepository refreshRepo, JwtTokenService jwt, ILogger<AuthService> log)
    {

        _users = users; _refreshRepo = refreshRepo; _jwt = jwt; _log = log;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        var exists = await _users.FindByEmailAsync(req.Email, ct);
        if (exists is not null)
            throw new ConflictException("Email already exists.", ErrorCodes.EmailExists);


        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        var user = await _users.AddAsync(new User
        {
            Username = req.Username,
            Email = req.Email,
            PasswordHash = hash,
            Role = "CUSTOMER",
            Active = true
        }, ct);

        return new AuthResult(user.Id, user.Username, user.Email, user.Role);
    }

    public async Task<(AuthResult user, string accessToken, string refreshToken, string refreshHash, string refreshFamily)>
        LoginAsync(LoginRequest req, string? ip, string? ua, CancellationToken ct = default)
    {
        var user = await _users.FindByEmailAsync(req.EmailOrUsername, ct)
                   ?? await _users.FindByUsernameAsync(req.EmailOrUsername, ct)
                   ?? throw new InvalidLoginException("Tài khoản hoặc mật khẩu không đúng", ErrorCodes.Unauthorized);
        _log.LogInformation("Login found user {@User}", new { user.Id, user.Username, user.Active });

        if (!user.Active) throw new InvalidLoginException("User is inactive.", ErrorCodes.Unauthorized);

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new InvalidLoginException("Tài khoản hoặc mật khẩu không đúng", ErrorCodes.BadRequest);

        var auth = new AuthResult(user.Id, user.Username, user.Email, user.Role);
        var accessToken = _jwt.CreateAccessToken(auth);

        // tạo refresh token (plaintext) + hash
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshHash = Sha256Hex(refreshToken);
        var family = Guid.NewGuid().ToString();

        var now = DateTime.UtcNow;
        await _refreshRepo.AddAsync(new RefreshToken
        {
            UserId = (ulong)user.Id,
            TokenHash = refreshHash,
            TokenFamily = family,
            IssuedAt = now,
            ExpiresAt = now.Add(_refreshTtl),
            Revoked = false,
            CreatedAt = now,
            UpdatedAt = now,
            IpAddress = ip,
            UserAgent = ua
        }, ct);

        return (auth, accessToken, refreshToken, refreshHash, family);
    }

    public async Task<(AuthResult user, string accessToken, string newRefreshToken, string newHash, string family)>
        RefreshAsync(long userId, string rawRefreshToken, string? ip, string? ua, CancellationToken ct = default)
    {
        var hash = Sha256Hex(rawRefreshToken);
        var token = await _refreshRepo.FindValidAsync((ulong)userId, hash, ct)
                    ?? throw new InvalidOperationException("Refresh token invalid or expired.");

        // rotate RT
        await _refreshRepo.RevokeAsync(token, ct: ct);
        var newRt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var newHash = Sha256Hex(newRt);
        var now = DateTime.UtcNow;
        await _refreshRepo.AddAsync(new RefreshToken
        {
            UserId = token.UserId,
            TokenHash = newHash,
            TokenFamily = token.TokenFamily,
            IssuedAt = now,
            ExpiresAt = now.Add(_refreshTtl),
            Revoked = false,
            CreatedAt = now,
            UpdatedAt = now,
            IpAddress = ip,
            UserAgent = ua
        }, ct);

        // bạn có thể lấy user lại từ repo nếu cần claim mới chi tiết hơn
        var auth = new AuthResult((long)token.UserId, "user", "email", "CUSTOMER");
        var access = _jwt.CreateAccessToken(auth);
        return (auth, access, newRt, newHash, token.TokenFamily ?? "");
    }

     /// <summary>
    /// Thu hồi tất cả refresh tokens còn hiệu lực thuộc 1 "family" của user.
    /// Dùng cho logout (một thiết bị) hoặc logout-all-tuỳ cách bạn cài family.
    /// </summary>
 public async Task<int> RevokeRefreshFamilyAsync(long userId, string family, CancellationToken ct = default)
{
    if (string.IsNullOrWhiteSpace(family)) return 0;
    var affected = await _refreshRepo.RevokeFamilyAsync((ulong)userId, family, ct);
    _log.LogInformation("Revoked {Count} refresh tokens for user {UserId} (family={Family})",
        affected, userId, family);
    return affected;
}


    public static string Sha256Hex(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString().ToUpperInvariant();
    }
}
