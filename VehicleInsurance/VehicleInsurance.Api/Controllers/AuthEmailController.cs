using Microsoft.AspNetCore.Mvc;
using VehicleInsurance.Application.EmailVerification;

namespace VehicleInsurance.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthEmailController : ControllerBase
{
    private readonly IEmailVerificationService _svc;

    public AuthEmailController(IEmailVerificationService svc) => _svc = svc;

    // Resend verification (có thể yêu cầu user đã đăng nhập, hoặc truyền email/username)
    public record ResendReq(long UserId, string Email, string Username);

    [HttpPost("resend-verification")]
    public async Task<IActionResult> Resend([FromBody] ResendReq req, CancellationToken ct)
    {
        await _svc.SendVerificationAsync(req.UserId, req.Email, req.Username, ct);
        return Ok(new { message = "Đã gửi email xác nhận (nếu thông tin hợp lệ)." });
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> Verify([FromQuery] string token, CancellationToken ct)
    {
        var ok = await _svc.VerifyAsync(token, ct);
        if (!ok) return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn/đã dùng." });
        return Ok(new { message = "Xác nhận email thành công." });
    }
}
