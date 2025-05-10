using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Devoted.Domain.Sql.Entity.Base;
using Devoted.GenericLibrary.GenericSql.Context;
using Devoted.GenericLibrary.GenericSql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Devoted.GenericLibrary.GenericSql.Repositories
{
    public class GenericSqlRepository<T> : IGenericSqlRepository<T>
        where T : BaseSqlEntity
    {
        private readonly SqlDbContext _context;
        private IQueryable<T> Queryable => _context.Set<T>().AsQueryable();

        public GenericSqlRepository(SqlDbContext context) => _context = context;

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await action();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task CreateAsync(T entity, bool assignTimestamps = true)
        {
            if (assignTimestamps)
                SetCreatedAtRecursive(entity, new HashSet<object>());

            await _context.Set<T>().AddAsync(entity);
        }

        public async Task CreateManyAsync(List<T> entities)
        {
            entities.ForEach(e => e.CreatedAt = DateTime.UtcNow);
            await _context.Set<T>().AddRangeAsync(entities);
        }

        public async Task<T?> FindAsync(
            Expression<Func<T, bool>> predicate,
            string include = "",
            bool includeDeleted = false,
            bool asNoTracking = true)
            => await FindAsync(predicate, x => x, include, includeDeleted, asNoTracking);

        public async Task<(IEnumerable<T> data, long total, long elementsLeft)> FindAllAsync(
            Expression<Func<T, bool>> predicate,
            int? limit = null,
            int? skip = null,
            string include = "",
            bool returnPaginationResult = false,
            bool includeDeleted = false,
            bool asNoTracking = true)
        {
            var (proj, total, left) = await FindAllAsync(
                predicate,
                x => x,
                limit,
                skip,
                include,
                returnPaginationResult,
                includeDeleted,
                asNoTracking);

            return (proj, total, left);
        }

        public async Task<TResult?> FindAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> projection,
            string include = "",
            bool includeDeleted = false,
            bool asNoTracking = true)
        {
            var baseQuery = asNoTracking ? Queryable.AsNoTracking() : Queryable;

            var query = ApplyIncludes(baseQuery, include)
                        .Where(e => includeDeleted || !e.IsDeleted)
                        .Where(predicate);

            return await query.Select(projection).FirstOrDefaultAsync();
        }

        public async Task<(IEnumerable<TResult> data, long total, long elementsLeft)> FindAllAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> projection,
            int? limit = null,
            int? skip = null,
            string include = "",
            bool returnPaginationResult = false,
            bool includeDeleted = false,
            bool asNoTracking = true)
        {
            var baseQuery = asNoTracking ? Queryable.AsNoTracking() : Queryable;

            IQueryable<T> query = ApplyIncludes(baseQuery, include)
                .Where(e => includeDeleted || !e.IsDeleted)
                .Where(predicate)
                .OrderByDescending(e => e.CreatedAt)
                .ThenBy(e => e.UpdatedAt);

            if (skip.HasValue) query = query.Skip(skip.Value);
            if (limit.HasValue) query = query.Take(limit.Value);

            if (!returnPaginationResult)
            {
                var list = await query.Select(projection).ToListAsync();
                return (list, 0, 0);
            }

            var total = await query.CountAsync();
            var data = await query.Select(projection).ToListAsync();
            var left = total - (skip ?? 0) - data.Count;

            return (data, total, left <= 0 ? 0 : left);
        }

        public void Update(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Set<T>().Update(entity);
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            var items = await Queryable.Where(predicate).ToListAsync();
            if (!items.Any()) return;

            items.ForEach(i =>
            {
                i.IsDeleted = true;
                i.UpdatedAt = DateTime.UtcNow;
            });

            _context.Set<T>().UpdateRange(items);
        }

        public async Task<bool> SoftDeleteAsync(string id) => await DeleteByIdSoft(id);

        public async Task<bool> SoftDeleteAsync(long id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            return await SoftDeleteEntityAsync(entity);
        }

        public async Task<bool> SoftDeleteAsync(Guid recordGuid)
        {
            var entity = await _context.Set<T>()
                                       .FirstOrDefaultAsync(e => e.RecordGuid == recordGuid);
            return await SoftDeleteEntityAsync(entity);
        }

        private async Task<bool> SoftDeleteEntityAsync(T? entity)
        {
            if (entity is null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Set<T>().Update(entity);
            await Task.CompletedTask;        // keeps signature async‑friendly
            return true;
        }

        private async Task<bool> DeleteByIdSoft(string id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity is null) return false;

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Set<T>().Update(entity);
            return true;
        }

        private static IQueryable<T> ApplyIncludes(IQueryable<T> query, string include)
        {
            if (!string.IsNullOrWhiteSpace(include))
            {
                foreach (var inc in include.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim()))
                {
                    query = query.Include(inc);
                }
            }
            return query;
        }

        private static void SetCreatedAtRecursive(object obj, HashSet<object> visited)
        {
            if (obj == null || obj is string || visited.Contains(obj)) return;
            visited.Add(obj);

            var type = obj.GetType();
            var createdProp = type.GetProperty("CreatedAt");
            if (createdProp?.PropertyType == typeof(DateTime))
                createdProp.SetValue(obj, DateTime.UtcNow);

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string)) continue;
                var value = prop.GetValue(obj);
                if (value == null) continue;

                if (value is IEnumerable enumerable && value is not string)
                {
                    foreach (var item in enumerable) SetCreatedAtRecursive(item, visited);
                }
                else
                {
                    SetCreatedAtRecursive(value, visited);
                }
            }
        }
    }
}
