using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Devoted.Domain.NoSql.Entity.Base;
using MongoDB.Driver;

namespace Devoted.GenericLibrary.GenericNoSql.Interfaces
{
    public interface IGenericNoSqlRepository<T> where T : BaseNosqlEntity
    {
        IMongoCollection<T> GetCollection();

        Task CreateAsync(T obj, bool assignProperties = true);

        Task CreateManyAsync(List<T> collection);

        Task<T> FindAsync(
            Expression<Func<T, bool>> funcExpression,
            bool includeDeleted = false);

        Task<T> FindAsync(
            Expression<Func<T, bool>> funcExpression,
            ProjectionDefinition<T> projection,
            bool includeDeleted = false);

        Task<(IEnumerable<T> data, long count, long elementsLeft)> FindAllAsync(
            Expression<Func<T, bool>> funcExpression,
            int? limit = null,
            int? skip = null,
            ProjectionDefinition<T> projection = null,
            bool returnPaginationResult = false,
            bool includeDeleted = false);

        Task Update(T document, bool isUpsert = false);

        Task UpdateManyAsync(List<T> collection);

        Task<bool> UpdateFields(
            Expression<Func<T, bool>> filterExpression,
            Dictionary<Expression<Func<T, object>>, object> fieldsToUpdate,
            bool isUpsert = false);

        Task<long> CountAsync(
            Expression<Func<T, bool>> funcExpression,
            bool includeDeleted = false);

        Task<bool> SoftDeleteAsync(string id);

        Task DeleteAsync(Expression<Func<T, bool>> funcExpression);

        Task DeleteManyAsync(Expression<Func<T, bool>> funcExpression);

        Task<bool> ChangeCount(
            Expression<Func<T, bool>> funcExpression,
            Expression<Func<T, object>> fieldToIncrement,
            int value);
    }
}
