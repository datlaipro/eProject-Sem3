using VehicleInsurance.Application.Vehicles.Dtos;

namespace VehicleInsurance.Application.Vehicles.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleResponse>> GetAllAsync(CancellationToken ct);
        
        Task<VehicleResponse?> GetByIdAsync(long id, CancellationToken ct);
        Task<VehicleResponse> CreateAsync(VehicleCreateRequest req, CancellationToken ct);
        Task<bool> UpdateAsync(long id, VehicleUpdateRequest req, CancellationToken ct);
        Task<bool> DeleteAsync(long id, CancellationToken ct);
    }
}
