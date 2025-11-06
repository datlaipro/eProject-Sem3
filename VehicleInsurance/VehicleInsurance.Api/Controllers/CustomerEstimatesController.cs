using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleInsurance.Application.Estimates.Services;
using VehicleInsurance.Application.Estimates.Dtos;
namespace VehicleInsurance.Api.Controllers
{
    [Authorize(Roles = "CUSTOMER")]
    [ApiController]
    [Route("api/customer/estimates")]
    public class CustomerEstimatesController : ControllerBase
    {
        private readonly IEstimateService _service;

        public CustomerEstimatesController(IEstimateService service) => _service = service;

        // ===================== GET ALL (Chỉ của khách hàng) =====================
        [HttpGet]
        public async Task<IActionResult> GetMyEstimates(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var customerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (items, total) = await _service.GetAllAsync(page, pageSize, customerId, ct);
            return Ok(new { page, pageSize, total, items });
        }

        // ===================== GET DETAIL (Chỉ của khách hàng hoặc báo giá public) =====================
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetMyEstimateDetail(long id, CancellationToken ct)
        {
            var customerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var dto = await _service.GetByIdAsync(id, ct);
            if (dto == null) return NotFound();

            // ✅ Nếu không phải báo giá của họ và cũng không phải public thì cấm
            if (dto.CustomerId != customerId && !dto.IsPublic)
                return Forbid();

            return Ok(dto);
        }

        // ===================== CREATE (Đăng ký báo giá mới) =====================
        [Authorize(Roles = "CUSTOMER")]
        [HttpPost]
        public async Task<IActionResult> CreateCustomerEstimate([FromBody] EstimateCreateRequest req, CancellationToken ct)
        {
            var customerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            req.CustomerId = customerId;
            req.CreatedByAdmin = false;

            var dto = await _service.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetMyEstimateDetail), new { id = dto.Id }, dto);
        }

    }
}
