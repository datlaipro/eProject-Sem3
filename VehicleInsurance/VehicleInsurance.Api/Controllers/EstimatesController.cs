// VehicleInsurance.Api/Controllers/EstimatesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleInsurance.Application.Estimates.Dtos;
using VehicleInsurance.Application.Estimates.Services;

namespace VehicleInsurance.Api.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    [Route("api/estimates")]
    public class EstimatesController : ControllerBase
    {
        private readonly IEstimateService _service;

        public EstimatesController(IEstimateService service) => _service = service;

        // VehicleInsurance.Api/Controllers/EstimatesController.cs
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] long? vehicleId = null,
            CancellationToken ct = default)
        {
            var (items, total) = await _service.GetAllAsync(page, pageSize, vehicleId, ct);
            return Ok(new { page, pageSize, total, items });
        }




        // GET /estimates/123
        [AllowAnonymous]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken ct)
        {
            var dto = await _service.GetByIdAsync(id, ct); // EstimateResponse (full)
            return dto is null ? NotFound() : Ok(dto);
        }

        // POST /estimates

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> CreateAdminEstimate([FromBody] EstimateCreateRequest req, CancellationToken ct)
        {
            req.CreatedByAdmin = true;

            var dto = await _service.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }


        // PATCH /estimates/123
        [HttpPatch("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] EstimateUpdateRequest req, CancellationToken ct)
        {
            var dto = await _service.UpdateAsync(id, req, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // DELETE /estimates/123
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            var ok = await _service.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
