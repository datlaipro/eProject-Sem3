using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleInsurance.Application.Vehicles.Dtos;
using VehicleInsurance.Application.Vehicles.Services;
using VehicleInsurance.Domain.Common.Exceptions;
using VehicleInsurance.Domain.Entity;
using VehicleInsurance.Infrastructure.Data;

namespace VehicleInsurance.Infrastructure.Vehicles.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(AppDbContext context, ILogger<VehicleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==============================================================
        // GET ALL VEHICLES
        // ==============================================================
        public async Task<IEnumerable<VehicleResponse>> GetAllAsync(CancellationToken ct)
        {
            _logger.LogInformation("🚗 [GetAllAsync] Fetching all vehicles from database...");

            try
            {
                var vehicles = await _context.Vehicles
                    .Select(v => new VehicleResponse
                    {
                        Id = v.Id,
                        CustomerId = v.CustomerId,
                        Name = v.Name,
                        OwnerName = v.OwnerName,
                        Model = v.Model,
                        Version = v.Version,
                        SeatCount = v.SeatCount, // ✅ thay Rate bằng SeatCount
                        BodyNumber = v.BodyNumber,
                        EngineNumber = v.EngineNumber,
                        VehicleNumber = v.VehicleNumber
                    })
                    .ToListAsync(ct);

                _logger.LogInformation("✅ [GetAllAsync] Retrieved {Count} vehicles", vehicles.Count);
                return vehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [GetAllAsync] Failed to retrieve vehicles");
                throw new ForbiddenAppException("vehicles not found");
            }
        }

        // ==============================================================
        // GET VEHICLE BY ID
        // ==============================================================
        public async Task<VehicleResponse?> GetByIdAsync(long id, CancellationToken ct)
        {
            _logger.LogInformation("🔍 [GetByIdAsync] Fetching vehicle ID = {Id}", id);

            try
            {
                var v = await _context.Vehicles.FindAsync(new object[] { id }, ct);

                if (v == null)
                {
                    _logger.LogWarning("⚠️ [GetByIdAsync] Vehicle ID = {Id} not found", id);
                    throw new ForbiddenAppException("vehicle not found");
                }

                _logger.LogInformation("✅ [GetByIdAsync] Found vehicle ID = {Id}", id);

                return new VehicleResponse
                {
                    Id = v.Id,
                    CustomerId = v.CustomerId,
                    Name = v.Name,
                    OwnerName = v.OwnerName,
                    Model = v.Model,
                    Version = v.Version,
                    SeatCount = v.SeatCount, // ✅ thay Rate bằng SeatCount
                    BodyNumber = v.BodyNumber,
                    EngineNumber = v.EngineNumber,
                    VehicleNumber = v.VehicleNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [GetByIdAsync] Failed to fetch vehicle ID = {Id}", id);
                throw new ForbiddenAppException("vehicle not found");
            }
        }

        // ==============================================================
        // CREATE VEHICLE
        // ==============================================================
        public async Task<VehicleResponse> CreateAsync(VehicleCreateRequest req, CancellationToken ct)
        {
            _logger.LogInformation("🆕 [CreateAsync] Creating vehicle for CustomerId = {CustomerId}", req.CustomerId);

            try
            {
                var entity = new Vehicle
                {
                    CustomerId = req.CustomerId,
                    Name = req.Name,
                    OwnerName = req.OwnerName,
                    Model = req.Model,
                    Version = req.Version,
                    SeatCount = req.SeatCount, // ✅ thêm trường mới
                    BodyNumber = req.BodyNumber,
                    EngineNumber = req.EngineNumber,
                    VehicleNumber = req.VehicleNumber,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Vehicles.Add(entity);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("✅ [CreateAsync] Vehicle created successfully with ID = {Id}", entity.Id);

                return new VehicleResponse
                {
                    Id = entity.Id,
                    CustomerId = entity.CustomerId,
                    Name = entity.Name,
                    OwnerName = entity.OwnerName,
                    Model = entity.Model,
                    Version = entity.Version,
                    SeatCount = entity.SeatCount, // ✅ thay Rate bằng SeatCount
                    BodyNumber = entity.BodyNumber,
                    EngineNumber = entity.EngineNumber,
                    VehicleNumber = entity.VehicleNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [CreateAsync] Exception while creating vehicle for CustomerId = {CustomerId}", req.CustomerId);
                Console.WriteLine($"🔥 VEHICLE CREATE ERROR: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                throw new ForbiddenAppException("cannot create vehicle");
            }
        }

        // ==============================================================
        // UPDATE VEHICLE
        // ==============================================================
        public async Task<bool> UpdateAsync(long id, VehicleUpdateRequest req, CancellationToken ct)
        {
            _logger.LogInformation("✏️ [UpdateAsync] Updating vehicle ID = {Id}", id);

            var v = await _context.Vehicles.FindAsync(new object[] { id }, ct);
            if (v == null)
            {
                _logger.LogWarning("⚠️ [UpdateAsync] Vehicle ID = {Id} not found", id);
                throw new ForbiddenAppException("vehicle not found");
            }

            v.Name = req.Name ?? v.Name;
            v.OwnerName = req.OwnerName ?? v.OwnerName;
            v.Model = req.Model ?? v.Model;
            v.Version = req.Version ?? v.Version;
            v.SeatCount = req.SeatCount ?? v.SeatCount; // ✅ cập nhật mới
            v.BodyNumber = req.BodyNumber ?? v.BodyNumber;
            v.EngineNumber = req.EngineNumber ?? v.EngineNumber;
            v.VehicleNumber = req.VehicleNumber ?? v.VehicleNumber;
            v.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("✅ [UpdateAsync] Vehicle ID = {Id} updated successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [UpdateAsync] Failed to update vehicle ID = {Id}", id);
                throw new ForbiddenAppException("cannot update vehicle");
            }
        }

        // ==============================================================
        // DELETE VEHICLE
        // ==============================================================
        public async Task<bool> DeleteAsync(long id, CancellationToken ct)
        {
            _logger.LogInformation("🗑️ [DeleteAsync] Deleting vehicle ID = {Id}", id);

            var v = await _context.Vehicles.FindAsync(new object[] { id }, ct);
            if (v == null)
            {
                _logger.LogWarning("⚠️ [DeleteAsync] Vehicle ID = {Id} not found", id);
                throw new ForbiddenAppException("vehicle not found");
            }

            try
            {
                _context.Vehicles.Remove(v);
                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("✅ [DeleteAsync] Vehicle ID = {Id} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [DeleteAsync] Failed to delete vehicle ID = {Id}", id);
                throw new ForbiddenAppException("cannot delete vehicle");
            }
        }
    }
}
