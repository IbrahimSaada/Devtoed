using Devoted.Domain.Sql.Dto;
using Devoted.Domain.Sql.Request.Product;
using Devoted.Domain.Sql.Response.Base;

namespace Devoted.Business.Interfaces
{
    public interface IProductService
    {
        Task<BaseResponse> CreateAsync(CreateProductRequest req, CancellationToken ct);
        Task<BaseResponse> BulkCreateAsync(IEnumerable<CreateProductRequest> reqs, CancellationToken ct);
        Task<BaseResponse> GetAsync(long id, CancellationToken ct);
        Task<BaseResponse> ListAsync(int skip, int limit, CancellationToken ct);
        Task<BaseResponse> UpdateAsync(long id, UpdateProductRequest req, CancellationToken ct);
        Task<BaseResponse> BulkUpdateAsync(IEnumerable<BulkUpdateDto> batch, CancellationToken ct);
        Task<BaseResponse> DeleteAsync(long id, CancellationToken ct);
        Task<BaseResponse> BulkDeleteAsync(IEnumerable<long> ids, CancellationToken ct);
        Task<BaseResponse> RestoreAsync(long id, CancellationToken ct);
        Task<BaseResponse> BulkRestoreAsync(IEnumerable<long> ids, CancellationToken ct);
    }
}
