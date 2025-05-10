using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Devoted.Domain.Enums;
using Devoted.Domain.NoSql.Entity.Base;
using Devoted.GenericLibrary.GenericNoSql.Context;
using Devoted.GenericLibrary.GenericNoSql.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Devoted.GenericLibrary.GenericNoSql.Repository
{
    public class GenericNoSqlRepository<T> : IGenericNoSqlRepository<T> where T : BaseNosqlEntity
    {
        private readonly NoSqlDbContext _context;

        public GenericNoSqlRepository(IServiceProvider serviceProvider)
        {
            _context = serviceProvider.GetRequiredService<NoSqlDbContext>();
        }

        public IMongoCollection<T> GetCollection()
        {
            return _context.database.GetCollection<T>(typeof(T).Name);
        }

        public async Task CreateAsync(T obj, bool assignProperties = true)
        {
            if (assignProperties)
            {
                AssignProperties(obj, new HashSet<object>());
            }
            await GetCollection().InsertOneAsync(obj);
        }

        public async Task CreateManyAsync(List<T> collection)
        {
            collection.ForEach(x => x.CreatedAt = DateTime.UtcNow);
            await GetCollection().InsertManyAsync(collection);
        }

        public async Task<T> FindAsync(
            Expression<Func<T, bool>> funcExpression,
            bool includeDeleted = false)
        {
            var combinedExpression = GenerateStateExpressions(funcExpression, includeDeleted);
            FilterDefinition<T> filter = Builders<T>.Filter.Where(combinedExpression);
            var result = await GetCollection().FindAsync(filter);
            return result.FirstOrDefault();
        }

        public async Task<T> FindAsync(
            Expression<Func<T, bool>> funcExpression,
            ProjectionDefinition<T> projection,
            bool includeDeleted = false)
        {
            var combinedExpression = GenerateStateExpressions(funcExpression, includeDeleted);
            FilterDefinition<T> filter = Builders<T>.Filter.Where(combinedExpression);
            var result = await GetCollection().Find(filter).Project<T>(projection).FirstOrDefaultAsync();
            return result;
        }

        public async Task<(IEnumerable<T> data, long count, long elementsLeft)> FindAllAsync(
            Expression<Func<T, bool>> funcExpression,
            int? limit = null,
            int? skip = null,
            ProjectionDefinition<T> projection = null,
            bool returnPaginationResult = false,
            bool includeDeleted = false)
        {
            var combinedExpression = GenerateStateExpressions(funcExpression, includeDeleted);
            FilterDefinition<T> filter = Builders<T>.Filter.Where(combinedExpression);
            var query = GetCollection().Find(filter);

            if (projection != null)
            {
                query = query.Project<T>(projection);
            }
            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }
            if (limit.HasValue)
            {
                query = query.Limit(limit.Value);
            }

            query = query.SortByDescending(t => t.CreatedAt)
                         .ThenBy(t => t.UpdatedAt);

            if (returnPaginationResult)
            {
                var totalDocuments = await CountAsync(funcExpression);
                var result = await query.ToListAsync();
                var totalDocumentsLeftToQuery = totalDocuments - (skip ?? 0) - (limit ?? totalDocuments);
                return (result, totalDocuments, totalDocumentsLeftToQuery <= 0 ? 0 : totalDocumentsLeftToQuery);
            }

            var data = await query.ToListAsync();
            return (data, 0, 0);
        }

        public async Task Update(T document, bool isUpsert = false)
        {
            var filtersBuilder = Builders<T>.Filter;
            document.UpdatedAt = DateTime.UtcNow;
            var filter = filtersBuilder.Where(x => x.Id == document.Id);
            var replaceOptions = new ReplaceOptions { IsUpsert = isUpsert };
            await GetCollection().ReplaceOneAsync(filter, document, replaceOptions);
        }

        public async Task<bool> UpdateFields(
            Expression<Func<T, bool>> filterExpression,
            Dictionary<Expression<Func<T, object>>, object> fieldsToUpdate,
            bool isUpsert = false)
        {
            fieldsToUpdate = fieldsToUpdate
                .Where(kvp => kvp.Value != null)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Expression<Func<T, object>> updatedAt = x => x.UpdatedAt;
            if (!fieldsToUpdate.ContainsKey(updatedAt))
            {
                fieldsToUpdate.Add(updatedAt, DateTime.UtcNow);
            }

            var update = Builders<T>.Update.Combine(
                fieldsToUpdate.Select(field => Builders<T>.Update.Set(field.Key, field.Value))
                               .ToArray());

            var updateOption = new UpdateOptions { IsUpsert = isUpsert };
            var result = await GetCollection().UpdateManyAsync(filterExpression, update, updateOption);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task UpdateManyAsync(List<T> collection)
        {
            var updates = new List<WriteModel<T>>();
            var filtersBuilder = Builders<T>.Filter;
            foreach (var document in collection)
            {
                document.UpdatedAt = DateTime.UtcNow;
                var filter = filtersBuilder.Where(x => x.Id == document.Id);
                updates.Add(new ReplaceOneModel<T>(filter, document));
            }
            await GetCollection().BulkWriteAsync(updates);
        }

        public async Task<long> CountAsync(
            Expression<Func<T, bool>> funcExpression,
            bool includeDeleted = false)
        {
            var combinedExpression = GenerateStateExpressions(funcExpression, includeDeleted);
            FilterDefinition<T> filter = Builders<T>.Filter.Where(combinedExpression);
            return await GetCollection().CountDocumentsAsync(filter);
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> funcExpression)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Where(funcExpression);
            await GetCollection().DeleteOneAsync(filter);
        }

        public async Task DeleteManyAsync(Expression<Func<T, bool>> funcExpression)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Where(funcExpression);
            await GetCollection().DeleteManyAsync(filter);
        }

        public async Task<bool> ChangeCount(
            Expression<Func<T, bool>> funcExpression,
            Expression<Func<T, object>> fieldToIncrement,
            int value)
        {
            var filter = Builders<T>.Filter.Where(funcExpression);
            var update = Builders<T>.Update.Inc(fieldToIncrement, value);
            var updatedResult = await GetCollection().UpdateOneAsync(filter, update);
            return updatedResult.IsAcknowledged && updatedResult.ModifiedCount > 0;
        }

        public async Task<bool> SoftDeleteAsync(string id)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Where(x => x.Id == id);
            var update = Builders<T>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
            var updatedResult = await GetCollection().UpdateOneAsync(filter, update);
            return updatedResult.IsAcknowledged && updatedResult.ModifiedCount > 0;
        }

        #region Private Methods

        private Expression<Func<T, bool>> GenerateStateExpressions(
            Expression<Func<T, bool>> funcExpression,
            bool includeDeleted)
        {
            Expression<Func<T, bool>> notDeleted = x => x.IsDeleted == includeDeleted;
            return CreateAndExpression(funcExpression, notDeleted, Condition.And);
        }

        private Expression<Func<T, bool>> CreateAndExpression(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2,
            Condition condition)
        {
            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);
            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);
            return condition == Condition.And
                ? Expression.Lambda<Func<T, bool>>(Expression.And(left, right), parameter)
                : Expression.Lambda<Func<T, bool>>(Expression.Or(left, right), parameter);
        }

        #endregion

        public void AssignProperties(object obj, HashSet<object> visited)
        {
            if (obj == null || obj is string)
                return;

            visited ??= new HashSet<object>();
            if (visited.Contains(obj))
                return;
            visited.Add(obj);

            var objType = obj.GetType();

            var dateProperty = objType.GetProperty("CreatedAt");
            if (dateProperty != null && dateProperty.PropertyType == typeof(DateTime))
            {
                dateProperty.SetValue(obj, DateTime.UtcNow);
            }

            foreach (var property in objType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                {
                    var propertyValue = property.GetValue(obj);
                    if (propertyValue == null)
                        continue;

                    if (propertyValue is IEnumerable enumerable && !(propertyValue is string))
                    {
                        foreach (var item in enumerable)
                        {
                            if (item != null)
                            {
                                AssignProperties(item, visited);
                            }
                        }
                    }
                    else
                    {
                        AssignProperties(propertyValue, visited);
                    }
                }
            }
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
}

