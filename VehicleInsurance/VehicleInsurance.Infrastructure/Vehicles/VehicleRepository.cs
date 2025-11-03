using Microsoft.EntityFrameworkCore;
using VehicleInsurance.Application.Vehicles.Interfaces;
using VehicleInsurance.Domain.Entity;
using VehicleInsurance.Infrastructure.Data;

namespace VehicleInsurance.Infrastructure.Vehicles
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Vehicle> _db;

        public VehicleRepository(AppDbContext context)
        {
            _context = context;
            _db = context.Set<Vehicle>();
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync(CancellationToken ct)
        {
            return await _db.AsNoTracking().ToListAsync(ct);
        }

        public async Task<Vehicle?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await _db.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
        }

        public async Task AddAsync(Vehicle entity, CancellationToken ct)
        {
            await _db.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Vehicle entity, CancellationToken ct)
        {
            _db.Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Vehicle entity, CancellationToken ct)
        {
            _db.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> ExistsAsync(long id, CancellationToken ct)
        {
            return await _db.AnyAsync(v => v.Id == id, ct);
        }
    }
}
