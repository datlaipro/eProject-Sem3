using Microsoft.AspNetCore.Mvc;
using VehicleInsurance.Application.Customers.Dtos;
using VehicleInsurance.Application.Customers.Services;
using Microsoft.AspNetCore.Authorization;// Authorize
namespace VehicleInsurance.Api.Controllers;


[Authorize(Roles = "ADMIN")]

[ApiController]
[Route("customers")]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _service;

    public CustomersController(CustomerService service)
    {
        _service = service;
    }
    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var dto = await _service.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerCreateRequest req, CancellationToken ct)
    {
        var dto = await _service.CreateAsync(req, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPatch("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] CustomerUpdateRequest req, CancellationToken ct)
    {
        var dto = await _service.UpdateAsync(id, req, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

}
