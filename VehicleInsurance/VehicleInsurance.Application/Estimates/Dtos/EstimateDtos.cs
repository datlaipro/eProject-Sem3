// VehicleInsurance.Application/Estimates/Dtos/EstimateDtos.cs
namespace VehicleInsurance.Application.Estimates.Dtos
{
     public class EstimateCreateRequest
    {
        public long? CustomerId { get; set; }          // Khách hàng tạo thì có
        public string? CustomerPhone { get; set; }     // Khách hàng tạo thì có
        public long? VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleModel { get; set; }
        public decimal? Rate { get; set; }
        public string? Warranty { get; set; }
        public string? PolicyType { get; set; }

        // ✅ Dùng để phân biệt admin/customer khi tạo
        public bool CreatedByAdmin { get; set; } = false;
    }

   public class EstimateUpdateRequest
{
    public long? VehicleId { get; set; }           // ✅ để cập nhật xe
    public string? VehicleName { get; set; }
    public string? VehicleModel { get; set; }
    public decimal? Rate { get; set; }
    public string? Warranty { get; set; }
    public string? PolicyType { get; set; }

    public bool? IsPublic { get; set; }            // ✅ cho phép admin bật/tắt public
    public string? Status { get; set; }            // ✅ cho phép admin cập nhật trạng thái
}


    public class EstimateResponse
    {
        public long Id { get; set; }
        public string EstimateNumber { get; set; } = default!;
        public long? CustomerId { get; set; }           // ✅ Thêm
        public string? CustomerPhone { get; set; }      // ✅ Thêm
        
        public long? VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleModel { get; set; }
        public decimal? Rate { get; set; }
        public string? Warranty { get; set; }
        public string? PolicyType { get; set; }
        public string Status { get; set; } = "PENDING"; // ✅ Thêm
        public bool IsPublic { get; set; }              // ✅ Thêm
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
