
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleInsurance.Application.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace VehicleInsurance.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth) => _auth = auth;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var user = await _auth.RegisterAsync(req, ct);
        return Ok(new { user.UserId, user.Username, user.Email });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers["User-Agent"].ToString();
        var (user, access, refresh, _, family) = await _auth.LoginAsync(req, ip, ua, ct);

        // Set cookies: access_token (ngắn), refresh_token (dài)
        SetCookie("access_token", access, TimeSpan.FromMinutes(15), sameSite: SameSiteMode.Strict);
        SetCookie("refresh_token", refresh, TimeSpan.FromDays(30), sameSite: SameSiteMode.Strict);

        // (tuỳ chọn) lưu family vào cookie để revoke family khi logout all
        SetCookie("rt_family", family, TimeSpan.FromDays(30), sameSite: SameSiteMode.Strict);

        return Ok(new { user.UserId, user.Username, user.Email, user.Role });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var accessUserId = TryGetUserIdFromCookieJwt(); // có thể null nếu access hết hạn
        var refreshCookie = Request.Cookies["refresh_token"];
        var family = Request.Cookies["rt_family"];

        if (string.IsNullOrEmpty(refreshCookie))
            return Unauthorized(new { message = "Missing refresh token cookie" });

        // Trường hợp access hết hạn, bạn cần userId (có thể đính kèm thêm cookie user_id lúc login)
        if (accessUserId is null)
            return Unauthorized(new { message = "Access expired and user id not available. Consider storing user_id cookie at login." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers["User-Agent"].ToString();

        var (user, access, newRefresh, _, _) =
            await _auth.RefreshAsync(accessUserId.Value, refreshCookie, ip, ua, ct);

        SetCookie("access_token", access, TimeSpan.FromMinutes(15), sameSite: SameSiteMode.Strict);
        SetCookie("refresh_token", newRefresh, TimeSpan.FromDays(30), sameSite: SameSiteMode.Strict);

        return Ok(new { user.UserId, user.Username, user.Email, user.Role });
    }

    [HttpPost("logout")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public async Task<IActionResult> Logout(CancellationToken ct)
{
    // Lấy userId từ JWT đã được middleware xác thực
    var uidStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
    long.TryParse(uidStr, out var uid);

    // Nếu bạn quản lý refresh-token theo “family”, nên thu hồi ở server:
    var family = Request.Cookies["rt_family"];
    if (!string.IsNullOrEmpty(family))
    {
        await _auth.RevokeRefreshFamilyAsync(uid, family, ct); // tự triển khai
    }

    // Xoá cookie phía client
    ExpireCookie("access_token");
    ExpireCookie("refresh_token");
    ExpireCookie("rt_family");

    return Ok(new { message = "Logged out" });
}


    private long? TryGetUserIdFromCookieJwt()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(id, out var uid)) return uid;
        }
        return null;
    }


    private void SetCookie(string name, string value, TimeSpan ttl, SameSiteMode sameSite)
    {
        Response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,           // bật HTTPS trong môi trường production
            SameSite = sameSite,
            Expires = DateTimeOffset.UtcNow.Add(ttl),
            Path = "/"
        });
    }

    private void ExpireCookie(string name)
    {
        Response.Cookies.Append(name, "", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
    }
}
