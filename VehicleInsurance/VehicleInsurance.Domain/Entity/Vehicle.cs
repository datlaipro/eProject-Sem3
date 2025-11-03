
using VehicleInsurance.Domain.Customers;
namespace VehicleInsurance.Domain.Entity
{
    public class Vehicle
    {
        public long Id { get; set; }
        public long? CustomerId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string BodyNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
 // 🔽 Thêm 2 dòng này
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Quan hệ
        public Customer? Customer { get; set; }
    }
}
