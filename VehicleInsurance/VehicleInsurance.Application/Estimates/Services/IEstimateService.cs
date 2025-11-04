// VehicleInsurance.Application/Estimates/Services/IEstimateService.cs
using VehicleInsurance.Application.Estimates.Dtos;

namespace VehicleInsurance.Application.Estimates.Services
{
    public interface IEstimateService
    {
        // Danh sách: chỉ còn filter theo vehicleId (customer đã bỏ khỏi Estimate)
        Task<(IEnumerable<EstimateListItemResponse> Items, int Total)> GetAllAsync(
            int page, int pageSize, long? vehicleId, CancellationToken ct);

        // Chi tiết
        Task<EstimateResponse?> GetByIdAsync(long id, CancellationToken ct);

        // Tạo / Cập nhật / Xóa
        Task<EstimateResponse> CreateAsync(EstimateCreateRequest req, CancellationToken ct);
        Task<EstimateResponse?> UpdateAsync(long id, EstimateUpdateRequest req, CancellationToken ct);
        Task<bool> DeleteAsync(long id, CancellationToken ct);

        // (nếu dùng PUT toàn phần)
        // Task<EstimateResponse?> UpdatePutAsync(long id, EstimatePutRequest req, CancellationToken ct);
    }
}
