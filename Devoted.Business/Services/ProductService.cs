using Devoted.Business.Error;
using Devoted.Business.Interfaces;
using Devoted.Domain.Sql.Dto;
using Devoted.Domain.Sql.Entity;
using Devoted.Domain.Sql.Request.Product;
using Devoted.Persistence.Sql.Interfaces;
using Microsoft.Extensions.Logging;

namespace Devoted.Business.Services
{
    public sealed class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly IAppUnitOfWork _uow;
        private readonly ILogger<ProductService> _log;

        public ProductService(IAppUnitOfWork uow,
                              IProductRepository repo,
                              ILogger<ProductService> log)
        {
            _uow = uow;
            _repo = repo;
            _log = log;
        }


        public async Task<ProductDto?> GetAsync(long id, CancellationToken ct)
        {
            var prod = await _repo.FindAsync(x => x.Id == id && !x.IsDeleted, asNoTracking: true);
            if (prod is null)
                throw new ItemNotFoundOrNullError($"Product {id} not found");
            return Map(prod);
        }

        public async Task<(IEnumerable<ProductDto>, long, long)>
            ListAsync(int skip, int limit, CancellationToken ct)
        {
            if (limit <= 0) throw new UserError("Limit must be greater than 0");

            var (rows, total, left) = await _repo.FindAllAsync(x => !x.IsDeleted,
                                                               limit, skip,
                                                               returnPaginationResult: true,
                                                               asNoTracking: true);
            return (rows.Select(Map), total, left);
        }


        public async Task<long> CreateAsync(CreateProductRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var entity = new Products { Name = req.Name, Price = req.Price };
            await _repo.CreateAsync(entity);
            await _uow.SaveChangesAsync();

            _log.LogInformation("Product {Id} created", entity.Id);
            return entity.Id;
        }

        public async Task<IEnumerable<long>> BulkCreateAsync(IEnumerable<CreateProductRequest> reqs, CancellationToken ct)
        {
            var entities = reqs.Select(r =>
            {
                ValidateRequest(r);
                return new Products { Name = r.Name, Price = r.Price };
            }).ToList();

            await _repo.CreateManyAsync(entities);
            await _uow.SaveChangesAsync();

            _log.LogInformation("Bulk created {Count} products", entities.Count);
            return entities.Select(e => e.Id);
        }


        public async Task<bool> UpdateAsync(long id, UpdateProductRequest req, CancellationToken ct)
        {
            ValidateRequest(req);

            var prod = await _repo.FindAsync(x => x.Id == id);
            if (prod is null)
                throw new ItemNotFoundOrNullError($"Product {id} not found");

            prod.Name = req.Name;
            prod.Price = req.Price;
            _repo.Update(prod);
            await _uow.SaveChangesAsync();

            _log.LogInformation("Product {Id} updated", id);
            return true;
        }

        public async Task<int> BulkUpdateAsync(IEnumerable<BulkUpdateDto> batch, CancellationToken ct)
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
            _log.LogInformation("Bulk updated {Count} products", updated);
            return updated;
        }


        public async Task<bool> SoftDeleteAsync(long id, CancellationToken ct)
        {
            if (!await _repo.SoftDeleteAsync(id))
                throw new ItemNotFoundOrNullError($"Product {id} not found");

            await _uow.SaveChangesAsync();
            _log.LogInformation("Product {Id} soft‑deleted", id);
            return true;
        }

        public async Task<int> BulkSoftDeleteAsync(IEnumerable<long> ids, CancellationToken ct)
        {
            int n = 0;
            foreach (var id in ids)
                if (await _repo.SoftDeleteAsync(id)) n++;

            if (n == 0)
                throw new ItemNotFoundOrNullError("No valid product ids supplied");

            await _uow.SaveChangesAsync();
            _log.LogInformation("Soft‑deleted {Count} products", n);
            return n;
        }

        public async Task<bool> RestoreAsync(long id, CancellationToken ct)
        {
            var p = await _repo.FindAsync(
            x => x.Id == id,
            includeDeleted: true
            );
            if (p is null || !p.IsDeleted)
                throw new ItemNotFoundOrNullError($"Product {id} not found or not deleted");

            p.IsDeleted = false;
            p.UpdatedAt = DateTime.UtcNow;
            _repo.Update(p);
            await _uow.SaveChangesAsync();

            _log.LogInformation("Product {Id} restored", id);
            return true;
        }

        public async Task<int> BulkRestoreAsync(IEnumerable<long> ids, CancellationToken ct)
        {
            int n = 0;
            foreach (var id in ids)
            {
                var p = await _repo.FindAsync(
                    x => x.Id == id,
                    includeDeleted:true);
                if (p is null || !p.IsDeleted) continue;

                p.IsDeleted = false;
                p.UpdatedAt = DateTime.UtcNow;
                _repo.Update(p);
                n++;
            }

            if (n == 0)
                throw new ItemNotFoundOrNullError("No products could be restored");

            await _uow.SaveChangesAsync();
            _log.LogInformation("Restored {Count} products", n);
            return n;
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

        private static ProductDto Map(Products p) => new(p.Id, p.Name, p.Price);
    }
}
