using VehicleInsurance.Application.Policies.Dtos;

namespace VehicleInsurance.Application.Policies.Services
{
    public interface IPolicyService
    {
        Task<PolicyResponse> PurchaseAsync(PurchasePolicyRequest req, CancellationToken ct);
        Task<PolicyResponse> PayAsync(long policyId, PayPolicyRequest req, CancellationToken ct);
        Task<PolicyResponse?> GetByIdAsync(long id, CancellationToken ct);
        Task<PolicyResponse> RenewAsync(long id, PolicyRenewRequest req, CancellationToken ct);

    }
}
