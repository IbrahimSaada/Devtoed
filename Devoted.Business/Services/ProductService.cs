using Devoted.Business.Error;
using Devoted.Business.Interfaces;
using Devoted.Domain.Sql.Dto;
using Devoted.Domain.Sql.Entity;
using Devoted.Domain.Sql.Request.Product;
using Devoted.Domain.Sql.Response.Base;
using Devoted.Persistence.Sql.Interfaces;
using Microsoft.Extensions.Logging;

namespace Devoted.Business.Services
{
    public sealed class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IAppUnitOfWork _uow;
        private readonly ILogger<ProductService> _log;

        public ProductService(
            IAppUnitOfWork uow,
            IProductRepository repo,
            ILogger<ProductService> log)
        {
            _uow = uow;
            _repo = repo;
            _log = log;
        }

        public async Task<BaseResponse> CreateAsync(CreateProductRequest req, CancellationToken ct)
        {
            ValidateRequest(req);
            var entity = new Products { Name = req.Name, Price = req.Price };
            await _repo.CreateAsync(entity);
            await _uow.SaveChangesAsync();

            _log.LogInformation("Created product {Id}", entity.Id);
            return new BaseResponse
            {
                Message = "Product created",
                Data = new { Id = entity.Id }
            };
        }

        public async Task<BaseResponse> BulkCreateAsync(IEnumerable<CreateProductRequest> reqs, CancellationToken ct)
        {
            var entities = reqs.Select(r => {
                ValidateRequest(r);
                return new Products { Name = r.Name, Price = r.Price };
            }).ToList();

            await _repo.CreateManyAsync(entities);
            await _uow.SaveChangesAsync();

            return new BaseResponse
            {
                Message = "Bulk created",
                Data = entities.Select(e => e.Id)
            };
        }

        public async Task<BaseResponse> GetAsync(long id, CancellationToken ct)
        {
            var p = await _repo.FindAsync(x => x.Id == id && !x.IsDeleted,
                                          asNoTracking: true);
            if (p is null) throw new ItemNotFoundOrNullError($"Product {id} not found");

            return new BaseResponse
            {
                Data = new ProductDto(p.Id, p.Name, p.Price)
            };
        }

        public async Task<BaseResponse> ListAsync(int skip, int limit, CancellationToken ct)
        {
            if (limit <= 0) throw new UserError("Limit must be greater than 0");

            var (rows, total, left) = await _repo.FindAllAsync(
                x => !x.IsDeleted, limit, skip,
                returnPaginationResult: true,
                asNoTracking: true);

            return new BaseResponse
            {
                Data = new
                {
                    Products = rows.Select(Map),
                    Pagination = new PaginationResult
                    {
                        TotalDocuments = total,
                        DocumentsLeft = left
                    }
                }
            };
        }

        public async Task<BaseResponse> UpdateAsync(long id, UpdateProductRequest req, CancellationToken ct)
        {
            ValidateRequest(req);
            var p = await _repo.FindAsync(x => x.Id == id);
            if (p is null) throw new ItemNotFoundOrNullError($"Product {id} not found");

            p.Name = req.Name;
            p.Price = req.Price;
            _repo.Update(p);
            await _uow.SaveChangesAsync();

            return new BaseResponse { Message = "Product updated" };
        }

        public async Task<BaseResponse> BulkUpdateAsync(IEnumerable<BulkUpdateDto> batch, CancellationToken ct)
        {
            int updated = 0;
            foreach (var item in batch)
            {
                ValidateRequest(new UpdateProductRequest(item.Name, item.Price));
                var p = await _repo.FindAsync(x => x.Id == item.Id);
                if (p is null) continue;

                p.Name = item.Name;
                p.Price = item.Price;
                _repo.Update(p);
                updated++;
            }
            if (updated > 0) await _uow.SaveChangesAsync();

            return new BaseResponse
            {
                Message = $"Updated {updated} products"
            };
        }

        public async Task<BaseResponse> DeleteAsync(long id, CancellationToken ct)
        {
            if (!await _repo.SoftDeleteAsync(id))
                throw new ItemNotFoundOrNullError($"Product {id} not found");
            await _uow.SaveChangesAsync();

            return new BaseResponse { Message = "Product deleted" };
        }

        public async Task<BaseResponse> BulkDeleteAsync(IEnumerable<long> ids, CancellationToken ct)
        {
            int n = 0;
            foreach (var id in ids)
                if (await _repo.SoftDeleteAsync(id)) n++;
            if (n == 0) throw new ItemNotFoundOrNullError("No valid product ids supplied");
            await _uow.SaveChangesAsync();

            return new BaseResponse
            {
                Message = $"Soft-deleted {n} products"
            };
        }

        public async Task<BaseResponse> RestoreAsync(long id, CancellationToken ct)
        {
            var p = await _repo.FindAsync(x => x.Id == id, includeDeleted: true);
            if (p is null || !p.IsDeleted)
                throw new ItemNotFoundOrNullError($"Product {id} not found or not deleted");

            p.IsDeleted = false;
            p.UpdatedAt = DateTime.UtcNow;
            _repo.Update(p);
            await _uow.SaveChangesAsync();

            return new BaseResponse { Message = "Product restored" };
        }

        public async Task<BaseResponse> BulkRestoreAsync(IEnumerable<long> ids, CancellationToken ct)
        {
            int n = 0;
            foreach (var id in ids)
            {
                var p = await _repo.FindAsync(x => x.Id == id, includeDeleted: true);
                if (p is null || !p.IsDeleted) continue;

                p.IsDeleted = false;
                p.UpdatedAt = DateTime.UtcNow;
                _repo.Update(p);
                n++;
            }
            if (n == 0) throw new ItemNotFoundOrNullError("No products could be restored");
            await _uow.SaveChangesAsync();

            return new BaseResponse
            {
                Message = $"Restored {n} products"
            };
        }

        private static void ValidateRequest(CreateProductRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                throw new UserError("Product name cannot be empty");
            if (req.Price <= 0)
                throw new UserError("Price must be positive");
        }

        private static void ValidateRequest(UpdateProductRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                throw new UserError("Product name cannot be empty");
            if (req.Price <= 0)
                throw new UserError("Price must be positive");
        }

        private static ProductDto Map(Products p)
            => new(p.Id, p.Name, p.Price);
    }
}
