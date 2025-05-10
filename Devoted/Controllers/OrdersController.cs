using Devoted.Business.Interfaces;
using Devoted.Domain.Sql.Request.Order;
using Microsoft.AspNetCore.Mvc;

namespace Devoted.Orders.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _svc;
        public OrdersController(IOrderService svc) => _svc = svc;

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateOrderRequest req, CancellationToken ct)
            => Ok(await _svc.CreateOrderAsync(req, ct));
    }
}
