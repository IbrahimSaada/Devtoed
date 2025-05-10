using Devoted.Domain.Sql.Entity.Base;
using Devoted.GenericLibrary.GenericSql.Interfaces;

namespace Devoted.Persistence.Sql.Interfaces
{
    public interface IAppUnitOfWork : IAsyncDisposable
    {
        IGenericSqlRepository<T> Repository<T>() where T : BaseSqlEntity;
        IProductRepository ProductRepository { get; }
        IOrderRepository OrderRepository { get; }
        Task SaveChangesAsync();
    }
}
