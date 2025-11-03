using VehicleInsurance.Domain.Entity;

namespace VehicleInsurance.Application.Vehicles.Interfaces
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAllAsync(CancellationToken ct);
        Task<Vehicle?> GetByIdAsync(long id, CancellationToken ct);
        Task AddAsync(Vehicle entity, CancellationToken ct);
        Task UpdateAsync(Vehicle entity, CancellationToken ct);
        Task DeleteAsync(Vehicle entity, CancellationToken ct);
        Task<bool> ExistsAsync(long id, CancellationToken ct);
    }
}
