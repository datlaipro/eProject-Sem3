using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleInsurance.Application.Policies.Dtos;
using VehicleInsurance.Application.Policies.Services;

namespace VehicleInsurance.Api.Controllers
{
    
    [ApiController]
    [Route("api/policies")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _service;

        public PoliciesController(IPolicyService service) => _service = service;

        /// <summary>Mua hợp đồng (tạo Policy + (nếu PayNow) tạo Billing và ACTIVE)</summary>
        [HttpPost("purchase")]
        public async Task<IActionResult> Purchase([FromBody] PurchasePolicyRequest req, CancellationToken ct)
        {
            var result = await _service.PurchaseAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Thanh toán cho hợp đồng (tạo Billing; nếu đang PENDING_PAYMENT -> ACTIVE)</summary>
        [HttpPost("{id:long}/pay")]
        public async Task<IActionResult> Pay(long id, [FromBody] PayPolicyRequest req, CancellationToken ct)
        {
            var result = await _service.PayAsync(id, req, ct);
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken ct)
        {
            var dto = await _service.GetByIdAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }
        [HttpPost("{id:long}/renew")]
        public async Task<IActionResult> Renew(long id, [FromBody] PolicyRenewRequest req, CancellationToken ct)
        {
            var result = await _service.RenewAsync(id, req, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

    }
}
