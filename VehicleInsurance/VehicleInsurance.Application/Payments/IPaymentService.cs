namespace VehicleInsurance.Application.Payments
{
    public interface IPaymentService
    {
        /// <summary>Create payment session/checkout for a billing. Return redirect url or payment token depending provider.</summary>
        Task<PaymentCreateResult> CreatePaymentAsync(PaymentCreateRequest req, CancellationToken ct);

        /// <summary>Handle provider callback (notify) - returns true if signature verified + processed</summary>
        Task<bool> HandleCallbackAsync(string rawBody, IDictionary<string,string?> headers, CancellationToken ct);
    }

    public record PaymentCreateRequest(long BillingId, decimal Amount, string Currency, string ReturnUrl, string NotifyUrl);
    public record PaymentCreateResult(bool Success, string? RedirectUrl, string? ProviderPaymentId, string? Message);
}
