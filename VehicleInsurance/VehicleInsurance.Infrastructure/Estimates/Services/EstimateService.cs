// VehicleInsurance.Infrastructure/Estimates/Services/EstimateService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleInsurance.Application.Estimates.Dtos;
using VehicleInsurance.Application.Estimates.Services;
using VehicleInsurance.Domain.Entity;
using VehicleInsurance.Infrastructure.Data;

namespace VehicleInsurance.Infrastructure.Estimates.Services
{
    public class EstimateService : IEstimateService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EstimateService> _logger;

        public EstimateService(AppDbContext context, ILogger<EstimateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // VehicleInsurance.Infrastructure/Estimates/Services/EstimateService.cs
        public async Task<(IEnumerable<EstimateListItemResponse> Items, int Total)> GetAllAsync(
            int page, int pageSize, long? vehicleId, CancellationToken ct)
        {
            var q = _context.Estimates.AsNoTracking().AsQueryable();
            if (vehicleId.HasValue) q = q.Where(x => x.VehicleId == vehicleId.Value);

            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EstimateListItemResponse
                {
                    Id = x.Id,
                    EstimateNumber = x.EstimateNumber,
                    VehicleId = x.VehicleId,
                    VehicleName = x.VehicleName,
                    VehicleModel = x.VehicleModel,
                    PolicyType = x.PolicyType,
                    Rate = x.Rate,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<EstimateResponse?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await _context.Estimates.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new EstimateResponse
                {
                    Id = x.Id,
                    EstimateNumber = x.EstimateNumber,
                    VehicleId = x.VehicleId,
                    VehicleName = x.VehicleName,
                    VehicleModel = x.VehicleModel,
                    Rate = x.Rate,
                    Warranty = x.Warranty,
                    PolicyType = x.PolicyType,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<EstimateResponse> CreateAsync(EstimateCreateRequest req, CancellationToken ct)
        {
            // ✅ Xác định role từ request (có thể truyền từ Controller)
            bool isAdmin = req.CreatedByAdmin;

            var est = new Estimate
            {
                EstimateNumber = await GenerateUniqueEstimateNumberAsync(ct),
                VehicleId = req.VehicleId,
                VehicleName = req.VehicleName,
                VehicleModel = req.VehicleModel,
                Rate = req.Rate,
                Warranty = req.Warranty,
                PolicyType = req.PolicyType,

                // ✅ Nếu admin tạo -> public & ready
                // ✅ Nếu customer tạo -> pending
                CustomerId = isAdmin ? null : req.CustomerId,
                CustomerPhone = isAdmin ? null : req.CustomerPhone,
                Status = isAdmin ? "READY" : "PENDING",
                IsPublic = isAdmin
            };

            _context.Estimates.Add(est);
            await _context.SaveChangesAsync(ct);

            return await GetByIdAsync(est.Id, ct)
                ?? throw new InvalidOperationException("Create failed unexpectedly.");
        }

        public async Task<(IEnumerable<EstimateListItemResponse> Items, int Total)> GetPublicAsync(
            int page, int pageSize, CancellationToken ct)
        {
            var q = _context.Estimates.AsNoTracking()
                .Where(x => x.IsPublic);

            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EstimateListItemResponse
                {
                    Id = x.Id,
                    EstimateNumber = x.EstimateNumber,
                    VehicleName = x.VehicleName,
                    VehicleModel = x.VehicleModel,
                    Rate = x.Rate,
                    PolicyType = x.PolicyType,
                    Status = x.Status,
                    IsPublic = x.IsPublic,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(ct);

            return (items, total);
        }



        public async Task<EstimateResponse?> UpdateAsync(long id, EstimateUpdateRequest req, CancellationToken ct)
        {
            // kiểm tra tồn tại mà KHÔNG đọc full entity
            var exists = await _context.Estimates.AsNoTracking()
                .AnyAsync(x => x.Id == id, ct);
            if (!exists) return null;

            // nếu cần validate vehicle
            if (req.VehicleId.HasValue)
            {
                var vehicleOk = await _context.Vehicles
                    .AsNoTracking()
                    .AnyAsync(v => v.Id == req.VehicleId.Value, ct);
                if (!vehicleOk) throw new InvalidOperationException("Vehicle not found.");
            }
            if (req.Rate.HasValue && req.Rate.Value < 0)
                throw new InvalidOperationException("Rate must be >= 0.");

            // Tạo "stub entity" chỉ có Id, attach rồi set từng property cần update
            var stub = new Estimate { Id = id };
            _context.Estimates.Attach(stub);

            if (req.VehicleId.HasValue)
            {
                stub.VehicleId = req.VehicleId;
                _context.Entry(stub).Property(x => x.VehicleId!).IsModified = true;
            }
            if (req.VehicleName is not null)
            {
                stub.VehicleName = req.VehicleName;
                _context.Entry(stub).Property(x => x.VehicleName!).IsModified = true;
            }
            if (req.VehicleModel is not null)
            {
                stub.VehicleModel = req.VehicleModel;
                _context.Entry(stub).Property(x => x.VehicleModel!).IsModified = true;
            }
            if (req.Rate.HasValue)
            {
                stub.Rate = req.Rate;
                _context.Entry(stub).Property(x => x.Rate!).IsModified = true;
            }
            if (req.Warranty is not null)
            {
                stub.Warranty = req.Warranty;
                _context.Entry(stub).Property(x => x.Warranty!).IsModified = true;
            }
            if (req.PolicyType is not null)
            {
                stub.PolicyType = req.PolicyType;
                _context.Entry(stub).Property(x => x.PolicyType!).IsModified = true;
            }

            await _context.SaveChangesAsync(ct);

            // trả lại bản chi tiết bằng projection (không load full entity)
            return await GetByIdAsync(id, ct);
        }


        public async Task<bool> DeleteAsync(long id, CancellationToken ct)
        {
            var est = await _context.Estimates.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (est == null) return false;
            _context.Estimates.Remove(est);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        private async Task<string> GenerateUniqueEstimateNumberAsync(CancellationToken ct)
        {
            // EST-YYYYMMDD-xxxxx
            string prefix = $"EST-{DateTime.UtcNow:yyyyMMdd}-";
            for (int i = 0; i < 5; i++)
            {
                var candidate = prefix + Random.Shared.Next(0, 99999).ToString("D5");
                var exists = await _context.Estimates
                    .AnyAsync(x => x.EstimateNumber == candidate, ct);
                if (!exists) return candidate;
            }
            // fallback theo Id tiếp theo
            var next = await _context.Estimates.AsNoTracking().OrderByDescending(x => x.Id)
                .Select(x => x.Id).FirstOrDefaultAsync(ct) + 1;
            return $"{prefix}{next:D5}";
        }
    }
}
