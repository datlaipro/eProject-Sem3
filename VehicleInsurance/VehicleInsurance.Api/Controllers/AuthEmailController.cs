using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using VehicleInsurance.Application.EmailVerification;

namespace VehicleInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class AuthEmailController : ControllerBase
{
    private readonly IEmailVerificationService _svc;
    private readonly ILogger<AuthEmailController> _log;

    public AuthEmailController(IEmailVerificationService svc, ILogger<AuthEmailController> log)
    {
        _svc = svc;
        _log = log;
    }

    // DTO: ràng buộc đầu vào cơ bản
public record ResendReq(long? UserId, string? Email, string? Username);

    /// <summary>
    /// Gửi lại email xác minh.
    /// </summary>
    [HttpPost("resend-verification")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> Resend([FromBody] ResendReq req, CancellationToken ct)
{
    try
    {
        await _svc.SendVerificationAsync(req.UserId, req.Email, req.Username, ct);
        return Ok(new { message = "Đã gửi email xác nhận (nếu thông tin hợp lệ)." });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { error = new { code = "GEN_BAD_REQUEST", message = ex.Message }, traceId = HttpContext.TraceIdentifier });
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { error = new { code = "USER_NOT_FOUND", message = ex.Message }, traceId = HttpContext.TraceIdentifier });
    }
    catch (InvalidOperationException ex)
    {
        return Conflict(new { error = new { code = "EMAIL_ALREADY_VERIFIED", message = ex.Message }, traceId = HttpContext.TraceIdentifier });
    }
    catch (Exception)
    {
        return StatusCode(500, new { error = new { code = "GEN_INTERNAL_SERVER_ERROR", message = "Internal server error" }, traceId = HttpContext.TraceIdentifier });
    }
}

    /// <summary>
    /// Xác minh email qua token trên query (?token=...)
    /// </summary>
    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status410Gone)] // token hết hạn/đã dùng -> 410 hợp lý
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Verify([FromQuery, Required] string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new
            {
                error = new { code = "GEN_BAD_REQUEST", message = "Thiếu token", errors = (object?)null },
                traceId = HttpContext.TraceIdentifier
            });
        }

        try
        {
            var ok = await _svc.VerifyAsync(token, ct);
            if (!ok)
            {
                // Tùy cách bạn implement VerifyAsync:
                // - Trả false khi token invalid/expired/used -> 410 Gone là lựa chọn “đúng ngữ nghĩa” hơn 400
                return StatusCode(StatusCodes.Status410Gone, new
                {
                    error = new { code = "TOKEN_INVALID_OR_EXPIRED", message = "Token không hợp lệ hoặc đã hết hạn/đã dùng.", errors = (object?)null },
                    traceId = HttpContext.TraceIdentifier
                });
            }

            return Ok(new { message = "Xác nhận email thành công." });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Verify email failed. traceId={TraceId}", HttpContext.TraceIdentifier);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = new { code = "GEN_INTERNAL_SERVER_ERROR", message = "Internal server error", errors = (object?)null },
                traceId = HttpContext.TraceIdentifier
            });
        }
    }
}
