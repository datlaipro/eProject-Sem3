using Microsoft.AspNetCore.Mvc;
using VehicleInsurance.Application.Vehicles.Dtos;
using VehicleInsurance.Application.Vehicles.Services;
using Microsoft.AspNetCore.Authorization;// Authorize

namespace VehicleInsurance.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _service;
        public VehiclesController(IVehicleService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _service.GetAllAsync(ct));

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken ct)
            => (await _service.GetByIdAsync(id, ct)) is { } v ? Ok(v) : NotFound();

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehicleCreateRequest req, CancellationToken ct)
        {
            var v = await _service.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id = v.Id }, v);
        }

        [HttpPatch("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] VehicleUpdateRequest req, CancellationToken ct)
            => await _service.UpdateAsync(id, req, ct) ? NoContent() : NotFound();

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
            => await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }
}
