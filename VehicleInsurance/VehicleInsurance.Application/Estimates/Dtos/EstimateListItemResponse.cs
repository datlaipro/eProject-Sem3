// VehicleInsurance.Application/Estimates/Dtos/EstimateListItemResponse.cs
namespace VehicleInsurance.Application.Estimates.Dtos
{
    public class EstimateListItemResponse
    {
        public long Id { get; set; }
        public string EstimateNumber { get; set; } = default!;
        public long? VehicleId { get; set; }

        // Điểm nhấn gợi ý (tuỳ bạn chọn hiển thị)
        public string? VehicleName { get; set; }
        public string? VehicleModel { get; set; }
        public string? PolicyType { get; set; }
        public decimal? Rate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
