namespace VehicleInsurance.Domain.Billings
{
    public class Billing
    {
        public long Id { get; set; }
        public string BillNo { get; set; } = default!;
        public long PolicyId { get; set; }
        public long CustomerId { get; set; }
        public long? VehicleId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly BillDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentRef { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
