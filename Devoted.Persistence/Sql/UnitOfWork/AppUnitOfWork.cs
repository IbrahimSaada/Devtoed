using Devoted.Domain.Sql.Entity.Base;
using Devoted.GenericLibrary.GenericSql.Interfaces;
using Devoted.Persistence.Sql.Context;
using Devoted.Persistence.Sql.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Devoted.Persistence.Sql.UnitOfWork
{
    public sealed class AppUnitOfWork : IAppUnitOfWork
    {
        private readonly AppDbContext _ctx;
        private readonly IServiceProvider _sp;
        private readonly Dictionary<Type, object> _cache = new();
        public IProductRepository ProductRepository { get; }

        public AppUnitOfWork(AppDbContext ctx, IServiceProvider sp)
        {
            _ctx = ctx;
            _sp = sp;
            ProductRepository = sp.GetRequiredService<IProductRepository>();
        }

        public IGenericSqlRepository<T> Repository<T>() where T : BaseSqlEntity
        {
            var type = typeof(T);
            if (!_cache.TryGetValue(type, out var repo))
            {
                repo = _sp.GetRequiredService<IGenericSqlRepository<T>>();
                _cache[type] = repo;
            }
            return (IGenericSqlRepository<T>)repo;
        }

        public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
        public ValueTask DisposeAsync() => _ctx.DisposeAsync();
    }
}
