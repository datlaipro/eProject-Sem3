using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleInsurance.Application.Payments;

namespace VehicleInsurance.Api.Controllers
{
    [Authorize]
    [ApiController]

    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _payment;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService payment, ILogger<PaymentsController> logger)
        {
            _payment = payment;
            _logger = logger;
        }

        // Tạo session thanh toán cho billing
        [HttpPost("create")]
        [Authorize] // hoặc cho phép anonymous tuỳ thiết kế
        public async Task<IActionResult> Create([FromBody] PaymentCreateRequestDto dto, CancellationToken ct)
        {
            var req = new PaymentCreateRequest(dto.BillingId, dto.Amount, dto.Currency ?? "VND", dto.ReturnUrl, dto.NotifyUrl);
            var r = await _payment.CreatePaymentAsync(req, ct);
            if (!r.Success) return BadRequest(new { error = r.Message });
            return Ok(r);
        }

        // Callback endpoint public mà Sepay sẽ gọi
        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(CancellationToken ct)
        {
            using var sr = new StreamReader(Request.Body);
            var raw = await sr.ReadToEndAsync(ct);
            var headers = Request.Headers.ToDictionary(k => k.Key, v => (string?)v.Value.ToString());

            var ok = await _payment.HandleCallbackAsync(raw, headers, ct);
            if (!ok) return BadRequest("Invalid signature or failed");
            // Sepay có thể yêu cầu trả về 200 ok hoặc một JSON xác nhận theo docs
            return Ok(new { status = "ok" });
        }
    }

    public record PaymentCreateRequestDto(long BillingId, decimal Amount, string? Currency, string ReturnUrl, string NotifyUrl);
}
