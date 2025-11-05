using Microsoft.Extensions.Logging;
using VehicleInsurance.Application.Payments;

namespace VehicleInsurance.Infrastructure.Payments
{
    public class FakePaymentService : IPaymentService
    {
        private readonly ILogger<FakePaymentService> _logger;
        private readonly IServiceProvider _sp;

        public FakePaymentService(ILogger<FakePaymentService> logger, IServiceProvider sp)
        {
            _logger = logger;
            _sp = sp;
        }

        public Task<PaymentCreateResult> CreatePaymentAsync(PaymentCreateRequest req, CancellationToken ct)
        {
            // Trả về URL giả để dev mở và simulate succeed
            var fakeUrl = $"https://fake-pay.local/simulate?billingId={req.BillingId}&amount={req.Amount}";
            _logger.LogInformation("Fake payment created: {Url}", fakeUrl);
            return Task.FromResult(new PaymentCreateResult(true, fakeUrl, $"FAKE-{Guid.NewGuid()}", "Fake payment created"));
        }

        public Task<bool> HandleCallbackAsync(string rawBody, IDictionary<string, string?> headers, CancellationToken ct)
        {
            // Ở local, mọi callback xem là hợp lệ (hoặc parse body để set status)
            _logger.LogInformation("Fake callback received. Body: {Body}", rawBody);
            return Task.FromResult(true);
        }
    }
}
