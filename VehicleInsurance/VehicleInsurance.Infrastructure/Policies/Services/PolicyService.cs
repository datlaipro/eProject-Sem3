using Microsoft.EntityFrameworkCore;
using VehicleInsurance.Application.Policies.Dtos;
using VehicleInsurance.Application.Policies.Services;
using VehicleInsurance.Domain.Policies;
using VehicleInsurance.Domain.Billings;
using VehicleInsurance.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace VehicleInsurance.Infrastructure.Policies.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PolicyService> _logger;

        public PolicyService(AppDbContext db, ILogger<PolicyService> logger)
        {
            _db = db;
            _logger = logger;
        }

        private static string NewPolicyNumber() => $"PL{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        private static string NewBillNo() => $"BI{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        public async Task<PolicyResponse?> GetByIdAsync(long id, CancellationToken ct)
        {
            var p = await _db.Policies.FirstOrDefaultAsync(x => x.Id == id, ct);
            return p is null ? null : ToResponse(p);
        }

        public async Task<PolicyResponse> PurchaseAsync(PurchasePolicyRequest req, CancellationToken ct)
        {
            // Kiểm tra tồn tại customer & vehicle
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == req.CustomerId, ct)
                           ?? throw new InvalidOperationException("Customer not found");

            var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == req.VehicleId, ct)
                          ?? throw new InvalidOperationException("Vehicle not found");

            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var start = req.StartDate ?? now;
            var end = start.AddDays(req.DurationDays <= 0 ? 365 : req.DurationDays);

            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var policy = new Policy
                {
                    PolicyNumber = $"PL{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    CustomerId = customer.Id,
                    VehicleId = vehicle.Id,
                    PolicyDate = now,
                    PolicyStartDate = start,
                    PolicyEndDate = end,
                    PolicyDurationDays = req.DurationDays <= 0 ? 365 : req.DurationDays,
                    Status = req.PayNow ? PolicyStatus.ACTIVE : PolicyStatus.PENDING_PAYMENT,

                    // 🔹 Lấy snapshot từ bảng VEHICLES
                    VehicleNumber = vehicle.VehicleNumber,
                    VehicleName = vehicle.Name,
                    VehicleModel = vehicle.Model,
                    VehicleVersion = vehicle.Version,
                    BodyNumber = vehicle.BodyNumber,
                    EngineNumber = vehicle.EngineNumber,
                    Rate = null,
                    Warranty = null,

                    // 🔹 Lấy thông tin KH (snapshot)
                    CustomerAddress = customer.Address,
                    CustomerPhone = customer.Phone,
                    CustomerAddressProof = customer.AddressProof
                };

                _db.Policies.Add(policy);
                await _db.SaveChangesAsync(ct);

                if (req.Amount > 0 && (req.PayNow || !string.IsNullOrWhiteSpace(req.PaymentMethod)))
                {
                    var bill = new Billing
                    {
                        BillNo = $"BI{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                        PolicyId = policy.Id,
                        CustomerId = policy.CustomerId,
                        VehicleId = policy.VehicleId,
                        Amount = req.Amount,
                        BillDate = now,
                        PaymentMethod = string.IsNullOrWhiteSpace(req.PaymentMethod) ? "CASH" : req.PaymentMethod,
                        PaymentRef = req.PaymentRef
                    };
                    _db.Billings.Add(bill);

                    if (policy.Status == PolicyStatus.PENDING_PAYMENT && req.PayNow)
                        policy.Status = PolicyStatus.ACTIVE;

                    await _db.SaveChangesAsync(ct);
                }

                await tx.CommitAsync(ct);
                return ToResponse(policy);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }


        public async Task<PolicyResponse> PayAsync(long policyId, PayPolicyRequest req, CancellationToken ct)
        {
            var policy = await _db.Policies.FirstOrDefaultAsync(x => x.Id == policyId, ct);
            if (policy is null) throw new InvalidOperationException("Policy not found");

            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var bill = new Billing
                {
                    BillNo = NewBillNo(),
                    PolicyId = policy.Id,
                    CustomerId = policy.CustomerId,
                    VehicleId = policy.VehicleId,
                    Amount = req.Amount,
                    BillDate = req.BillDate,
                    PaymentMethod = req.PaymentMethod,
                    PaymentRef = req.PaymentRef
                };
                _db.Billings.Add(bill);

                if (policy.Status == PolicyStatus.PENDING_PAYMENT)
                    policy.Status = PolicyStatus.ACTIVE;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ToResponse(policy);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private static PolicyResponse ToResponse(Policy p) => new()
        {
            Id = p.Id,
            PolicyNumber = p.PolicyNumber,
            Status = p.Status.ToString(),
            Start = p.PolicyStartDate,
            End = p.PolicyEndDate,
            DurationDays = p.PolicyDurationDays,
            Rate = p.Rate,
            Warranty = p.Warranty,
            CustomerId = p.CustomerId,
            VehicleId = p.VehicleId
        };
        public async Task<PolicyResponse> RenewAsync(long id, PolicyRenewRequest req, CancellationToken ct)
        {
            var oldPolicy = await _db.Policies
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (oldPolicy is null)
                throw new InvalidOperationException("Policy not found.");

            if (oldPolicy.Status != PolicyStatus.ACTIVE && oldPolicy.Status != PolicyStatus.EXPIRED)
                throw new InvalidOperationException("Only active or expired policies can be renewed.");

            var newStart = oldPolicy.PolicyEndDate?.AddDays(1) ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var newEnd = newStart.AddDays(req.DurationDays <= 0 ? 365 : req.DurationDays);

            using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var newPolicy = new Policy
                {
                    PolicyNumber = $"PL{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    CustomerId = oldPolicy.CustomerId,
                    VehicleId = oldPolicy.VehicleId,
                    PolicyDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    PolicyStartDate = newStart,
                    PolicyEndDate = newEnd,
                    PolicyDurationDays = req.DurationDays,
                    Status = req.PayNow ? PolicyStatus.ACTIVE : PolicyStatus.PENDING_PAYMENT,

                    VehicleNumber = oldPolicy.VehicleNumber,
                    VehicleName = oldPolicy.VehicleName,
                    VehicleModel = oldPolicy.VehicleModel,
                    VehicleVersion = oldPolicy.VehicleVersion,
                    BodyNumber = oldPolicy.BodyNumber,
                    EngineNumber = oldPolicy.EngineNumber,
                    Rate = oldPolicy.Rate,
                    Warranty = oldPolicy.Warranty,

                    CustomerAddress = oldPolicy.CustomerAddress,
                    CustomerPhone = oldPolicy.CustomerPhone,
                    CustomerAddressProof = oldPolicy.CustomerAddressProof
                };

                _db.Policies.Add(newPolicy);
                await _db.SaveChangesAsync(ct);

                if (req.Amount > 0)
                {
                    var bill = new Billing
                    {
                        BillNo = $"BI{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                        PolicyId = newPolicy.Id,
                        CustomerId = newPolicy.CustomerId,
                        VehicleId = newPolicy.VehicleId,
                        Amount = req.Amount,
                        BillDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        PaymentMethod = req.PaymentMethod ?? "CASH",
                        PaymentRef = req.PaymentRef
                    };
                    _db.Billings.Add(bill);

                    if (newPolicy.Status == PolicyStatus.PENDING_PAYMENT && req.PayNow)
                        newPolicy.Status = PolicyStatus.ACTIVE;

                    await _db.SaveChangesAsync(ct);
                }

                await tx.CommitAsync(ct);
                return ToResponse(newPolicy);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

    }
}
