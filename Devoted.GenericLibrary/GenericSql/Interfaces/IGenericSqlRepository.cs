using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Devoted.Domain.Sql.Entity.Base;

namespace Devoted.GenericLibrary.GenericSql.Interfaces
{
    public interface IGenericSqlRepository<T> where T : BaseSqlEntity
    {
        Task SaveChangesAsync();
        Task ExecuteInTransactionAsync(Func<Task> action);
        Task CreateAsync(T entity, bool assignTimestamps = true);
        Task CreateManyAsync(List<T> entities);
        void Update(T entity);
        Task DeleteAsync(Expression<Func<T, bool>> predicate);
        Task<bool> SoftDeleteAsync(string id);

        Task<bool> SoftDeleteAsync(long id);
        Task<bool> SoftDeleteAsync(Guid recordGuid);

        Task<T?> FindAsync(
            Expression<Func<T, bool>> predicate,
            string include = "",
            bool includeDeleted = false,
            bool asNoTracking = true);

        Task<(IEnumerable<T> data, long total, long elementsLeft)> FindAllAsync(
            Expression<Func<T, bool>> predicate,
            int? limit = null,
            int? skip = null,
            string include = "",
            bool returnPaginationResult = false,
            bool includeDeleted = false,
            bool asNoTracking = true);

        Task<TResult?> FindAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> projection,
            string include = "",
            bool includeDeleted = false,
            bool asNoTracking = true);

        Task<(IEnumerable<TResult> data, long total, long elementsLeft)> FindAllAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> projection,
            int? limit = null,
            int? skip = null,
            string include = "",
            bool returnPaginationResult = false,
            bool includeDeleted = false,
            bool asNoTracking = true);
    }
}
