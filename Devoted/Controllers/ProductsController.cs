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

        public ProductsController(IProductService svc)
        {
            _svc = svc;
        }

        /* --------------------- Create --------------------- */

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductRequest req,
            CancellationToken ct)
        {
            var id = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(Get),
                new { id },
                new BaseResponse
                {
                    Message = "Product created",
                    Data = new { Id = id }
                });
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate(
            [FromBody] IEnumerable<CreateProductRequest> reqs,
            CancellationToken ct)
        {
            var ids = await _svc.BulkCreateAsync(reqs, ct);
            return Ok(new BaseResponse
            {
                Message = "Bulk created",
                Data = ids
            });
        }

        /* --------------------- Read ---------------------- */

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id, CancellationToken ct)
        {
            var dto = await _svc.GetAsync(id, ct);          // throws if not found
            return Ok(new BaseResponse { Data = dto });
        }

        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] PaginationRequest q,
            CancellationToken ct)
        {
            var (data, total, left) = await _svc.ListAsync(q.Skip, q.Limit, ct);
            return Ok(new BaseResponse
            {
                Data = new
                {
                    Products = data,
                    Pagination = new PaginationResult
                    {
                        TotalDocuments = total,
                        DocumentsLeft = left
                    }
                }
            });
        }

        /* --------------------- Update -------------------- */

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(
            long id,
            [FromBody] UpdateProductRequest req,
            CancellationToken ct)
        {
            await _svc.UpdateAsync(id, req, ct);   // throws if not found
            return NoContent();
        }

        [HttpPut("bulk")]
        public async Task<IActionResult> BulkUpdate(
            [FromBody] IEnumerable<BulkUpdateDto> batch,
            CancellationToken ct)
        {
            var n = await _svc.BulkUpdateAsync(batch, ct);
            return Ok(new BaseResponse
            {
                Message = $"Updated {n} products"
            });
        }

        /* -------------------- Delete -------------------- */

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            await _svc.SoftDeleteAsync(id, ct);    // throws if not found
            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkDelete(
            [FromBody] BulkIdRequest ids,
            CancellationToken ct)
        {
            var n = await _svc.BulkSoftDeleteAsync(ids.Ids, ct);
            return Ok(new BaseResponse { Message = $"Soft‑deleted {n} products" });
        }

        /* ------------------- Restore -------------------- */

        [HttpPost("{id:long}/restore")]
        public async Task<IActionResult> Restore(long id, CancellationToken ct)
        {
            await _svc.RestoreAsync(id, ct);      // throws if not found / not deleted
            return Ok(new BaseResponse { Message = "Restored" });
        }

        [HttpPost("bulk/restore")]
        public async Task<IActionResult> BulkRestore(
            [FromBody] BulkIdRequest ids,
            CancellationToken ct)
        {
            var n = await _svc.BulkRestoreAsync(ids.Ids, ct);
            return Ok(new BaseResponse { Message = $"Restored {n} products" });
        }
    }
}
