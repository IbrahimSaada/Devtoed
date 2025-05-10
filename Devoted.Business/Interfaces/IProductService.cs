using Devoted.Domain.Sql.Dto;
using Devoted.Domain.Sql.Request.Product;

namespace Devoted.Business.Interfaces
{
    public interface IProductService
    {
        /* READ */
        Task<ProductDto?> GetAsync(long id, CancellationToken ct);
        Task<(IEnumerable<ProductDto> Data, long Total, long Left)>
            ListAsync(int skip, int limit, CancellationToken ct);

        /* CREATE */
        Task<long> CreateAsync(CreateProductRequest req, CancellationToken ct);
        Task<IEnumerable<long>> BulkCreateAsync(IEnumerable<CreateProductRequest> reqs, CancellationToken ct);

        /* UPDATE */
        Task<bool> UpdateAsync(long id, UpdateProductRequest req, CancellationToken ct);
        Task<int> BulkUpdateAsync(IEnumerable<BulkUpdateDto> batch, CancellationToken ct);

        /* DELETE / RESTORE */
        Task<bool> SoftDeleteAsync(long id, CancellationToken ct);
        Task<int> BulkSoftDeleteAsync(IEnumerable<long> ids, CancellationToken ct);

        Task<bool> RestoreAsync(long id, CancellationToken ct);
        Task<int> BulkRestoreAsync(IEnumerable<long> ids, CancellationToken ct);
    }
}
