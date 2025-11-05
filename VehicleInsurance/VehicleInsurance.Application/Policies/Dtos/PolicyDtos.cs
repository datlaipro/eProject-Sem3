namespace VehicleInsurance.Application.Policies.Dtos
{
    public class PurchasePolicyRequest
{
    public long CustomerId { get; set; }
    public long VehicleId { get; set; }

    public DateOnly? StartDate { get; set; }
    public int DurationDays { get; set; } = 365;

    // hóa đơn
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentRef { get; set; }
    public bool PayNow { get; set; } = false;  // true => ACTIVE ngay
}


    public class PolicyResponse
    {
        public long Id { get; set; }
        public string PolicyNumber { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateOnly? Start { get; set; }
        public DateOnly? End { get; set; }
        public int? DurationDays { get; set; }
        public decimal? Rate { get; set; }
        public string? Warranty { get; set; }
        public long CustomerId { get; set; }
        public long? VehicleId { get; set; }
    }

    public class PayPolicyRequest
    {
        public decimal Amount { get; set; }
        public DateOnly BillDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public string PaymentMethod { get; set; } = "CASH";
        public string? PaymentRef { get; set; }
    }
    public class PolicyRenewRequest
{
    public int DurationDays { get; set; } = 365;
    public decimal Amount { get; set; }
    public bool PayNow { get; set; } = false;
    public string? PaymentMethod { get; set; }
    public string? PaymentRef { get; set; }
}

}
