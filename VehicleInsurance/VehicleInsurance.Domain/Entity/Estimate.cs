// VehicleInsurance.Domain/Entity/Estimate.cs
using System;
using VehicleInsurance.Domain.Customers;

namespace VehicleInsurance.Domain.Entity
{
    public class Estimate
    {
        public long Id { get; set; }
        public string EstimateNumber { get; set; } = default!;

        // ✅ CustomerId có thể null (admin tạo)
        public long? CustomerId { get; set; }
        public long? VehicleId { get; set; }

        public string? CustomerPhone { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleModel { get; set; }

        public decimal? Rate { get; set; }
        public string? Warranty { get; set; }
        public string? PolicyType { get; set; }

        // ✅ Trạng thái & công khai
        public string Status { get; set; } = "PENDING"; // Customer: PENDING | Admin: READY
        public bool IsPublic { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Vehicle? Vehicle { get; set; }
        public Customer? Customer { get; set; }
    }
}
