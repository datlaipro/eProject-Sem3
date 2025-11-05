using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VehicleInsurance.Application.Payments;

namespace VehicleInsurance.Infrastructure.Payments
{
    public class SepaySettings
    {
        public string ApiBaseUrl { get; set; } = "";
        public string MerchantId { get; set; } = "";
        public string ApiKey { get; set; } = "";      // hoặc secret
        public bool UseSandbox { get; set; } = false;
    }

    public class SepayPaymentService : IPaymentService
    {
        private readonly HttpClient _http;
        private readonly SepaySettings _cfg;
        private readonly ILogger<SepayPaymentService> _logger;

        public SepayPaymentService(HttpClient http, IOptions<SepaySettings> cfg, ILogger<SepayPaymentService> logger)
        {
            _http = http;
            _cfg = cfg.Value;
            _logger = logger;
        }

        public async Task<PaymentCreateResult> CreatePaymentAsync(PaymentCreateRequest req, CancellationToken ct)
        {
            // Build payload theo API SePay (tham khảo docs của SePay)
            var payload = new
            {
                merchant_id = _cfg.MerchantId,
                amount = req.Amount,
                currency = "VND",
                return_url = req.ReturnUrl,
                notify_url = req.NotifyUrl,
                order_id = req.BillingId.ToString(),
                // ... các trường bắt buộc khác
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            // nếu cần header auth:
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cfg.ApiKey);

            var res = await _http.PostAsync($"{_cfg.ApiBaseUrl}/payments", new StringContent(json, Encoding.UTF8, "application/json"), ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError("Sepay create payment failed: {Status} {Body}", res.StatusCode, body);
                return new PaymentCreateResult(false, null, null, "Sepay error");
            }

            // parse response theo SePay (ví dụ có trường redirect_url và payment_id)
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            var redirect = doc.RootElement.GetProperty("redirect_url").GetString();
            var pid = doc.RootElement.GetProperty("payment_id").GetString();
            return new PaymentCreateResult(true, redirect, pid, "OK");
        }

        public async Task<bool> HandleCallbackAsync(string rawBody, IDictionary<string, string?> headers, CancellationToken ct)
        {
            // TODO: verify signature from headers / body using _cfg.ApiKey or secret
            // Example: lấy header "X-Sepay-Signature" và verify HMAC SHA256(rawBody, secret)
            // Nếu verify ok -> xử lý DB update billing + policy status
            _logger.LogInformation("Sepay callback raw: {Raw}", rawBody);

            // Implement verification and processing here
            return await Task.FromResult(true);
        }
    }
}
