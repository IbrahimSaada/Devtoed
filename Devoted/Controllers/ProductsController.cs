using Devoted.Business.Interfaces;
using Devoted.Domain.Sql.Dto;
using Devoted.Domain.Sql.Request.Base;
using Devoted.Domain.Sql.Request.Product;
using Devoted.Domain.Sql.Response.Base;
using Microsoft.AspNetCore.Mvc;

namespace Devoted.Products.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _svc;
        public ProductsController(IProductService svc) => _svc = svc;

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateProductRequest req, CancellationToken ct)
            => Ok(await _svc.CreateAsync(req, ct));

        [HttpPost("bulkCreate")]
        public async Task<IActionResult> BulkCreate(IEnumerable<CreateProductRequest> reqs, CancellationToken ct)
            => Ok(await _svc.BulkCreateAsync(reqs, ct));

        [HttpGet("get/{id:long}")]
        public async Task<IActionResult> Get(long id, CancellationToken ct)
            => Ok(await _svc.GetAsync(id, ct));

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest q, CancellationToken ct)
            => Ok(await _svc.ListAsync(q.Skip, q.Limit, ct));

        [HttpPut("update/{id:long}")]
        public async Task<IActionResult> Update(long id, UpdateProductRequest req, CancellationToken ct)
            => Ok(await _svc.UpdateAsync(id, req, ct));

        [HttpPut("bulkUpdate")]
        public async Task<IActionResult> BulkUpdate(IEnumerable<BulkUpdateDto> batch, CancellationToken ct)
            => Ok(await _svc.BulkUpdateAsync(batch, ct));

        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
            => Ok(await _svc.DeleteAsync(id, ct));

        [HttpDelete("bulkDelete")]
        public async Task<IActionResult> BulkDelete([FromBody] BulkIdRequest ids, CancellationToken ct)
            => Ok(await _svc.BulkDeleteAsync(ids.Ids, ct));

        [HttpPost("restore/{id:long}")]
        public async Task<IActionResult> Restore(long id, CancellationToken ct)
            => Ok(await _svc.RestoreAsync(id, ct));

        [HttpPost("bulkRestore")]
        public async Task<IActionResult> BulkRestore([FromBody] BulkIdRequest ids, CancellationToken ct)
            => Ok(await _svc.BulkRestoreAsync(ids.Ids, ct));
    }
}
