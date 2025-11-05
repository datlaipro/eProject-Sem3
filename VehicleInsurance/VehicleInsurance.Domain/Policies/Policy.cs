namespace VehicleInsurance.Domain.Policies
{
    public enum PolicyStatus
    {
        PENDING_PAYMENT,  // chờ thanh toán
        ACTIVE,           // đã hiệu lực
        EXPIRED,          // hết hạn
        LAPSED,           // mất hiệu lực do quá hạn thanh toán gia hạn
        CANCELLED         // hủy
    }

    public class Policy
    {
        public long Id { get; set; }
        public string PolicyNumber { get; set; } = default!;

        public long CustomerId { get; set; }
        public long? VehicleId { get; set; }

        public DateOnly? PolicyDate { get; set; }
        public DateOnly? PolicyStartDate { get; set; }
        public DateOnly? PolicyEndDate { get; set; }
        public int? PolicyDurationDays { get; set; }
        public PolicyStatus Status { get; set; } = PolicyStatus.PENDING_PAYMENT;

        public string? VehicleNumber { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleModel { get; set; }
        public string? VehicleVersion { get; set; }
        public decimal? Rate { get; set; }
        public string? Warranty { get; set; }
        public string? BodyNumber { get; set; }
        public string? EngineNumber { get; set; }

        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddressProof { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
