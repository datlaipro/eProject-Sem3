// VehicleInsurance.Domain/Entity/Estimate.cs
using System;
using VehicleInsurance.Domain.Entity; // nếu bạn để Customer/Vehicle tại đây
using VehicleInsurance.Domain.Customers;
namespace VehicleInsurance.Domain.Entity
{
    public class Estimate
    {
        public long Id { get; set; }
        public string EstimateNumber { get; set; } = default!;

        public long? VehicleId { get; set; }

        public string? VehicleName { get; set; }
        public string? VehicleModel { get; set; }
        public decimal? Rate { get; set; }
        public string? Warranty { get; set; }
        public string? PolicyType { get; set; }

        public DateTime CreatedAt { get; set; }  // mapped to TIMESTAMP
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Vehicle? Vehicle { get; set; }
    }
}
