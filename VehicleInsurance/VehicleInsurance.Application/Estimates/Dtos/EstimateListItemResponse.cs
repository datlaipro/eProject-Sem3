// VehicleInsurance.Application/Estimates/Dtos/EstimateListItemResponse.cs
namespace VehicleInsurance.Application.Estimates.Dtos
{
    public class EstimateListItemResponse
    {
        public long Id { get; set; }
        public string EstimateNumber { get; set; } = default!;
        public string? VehicleName { get; set; }
            public long? VehicleId { get; set; }           // ✅ để cập nhật xe

        public string? VehicleModel { get; set; }
        public decimal? Rate { get; set; }
        public string? PolicyType { get; set; }
        public string Status { get; set; } = "PENDING"; // ✅ thêm
        public bool IsPublic { get; set; }              // ✅ thêm
        public DateTime CreatedAt { get; set; }
    }
}