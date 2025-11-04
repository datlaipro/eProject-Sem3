// VehicleInsurance.Application/Estimates/Dtos/EstimateDtos.cs
namespace VehicleInsurance.Application.Estimates.Dtos
{
    public class EstimateCreateRequest
    {
        public long? VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleModel { get; set; }
        public decimal? Rate { get; set; }
        public string? Warranty { get; set; }
        public string? PolicyType { get; set; }
    }

    public class EstimateUpdateRequest
    {
        public string? VehicleName { get; set; }
        public string? VehicleModel { get; set; }
        public decimal? Rate { get; set; }
        public string? Warranty { get; set; }
        public string? PolicyType { get; set; }
        public long? VehicleId { get; set; }
    }

    public class EstimateResponse
    {
        public long Id { get; set; }
        public string EstimateNumber { get; set; } = default!;
        public long? VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleModel { get; set; }
        public decimal? Rate { get; set; }
        public string? Warranty { get; set; }
        public string? PolicyType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
