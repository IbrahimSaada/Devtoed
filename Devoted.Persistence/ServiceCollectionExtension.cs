using Devoted.GenericLibrary.GenericSql.Context;   // ← for SqlDbContext
using Devoted.GenericLibrary.GenericSql.Interfaces;
using Devoted.Persistence.Sql.Context;
using Devoted.Persistence.Sql.GenericRepository;
using Devoted.Persistence.Sql.Interfaces;
using Devoted.Persistence.Sql.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Devoted.Persistence
{
    public static class PersistenceServiceCollectionExtensions
    {
        public static IServiceCollection RegisterPersistenceServices(
            this IServiceCollection services,
            IConfiguration cfg)
        {
            // Register AppDbContext _as_ SqlDbContext
            services.AddDbContext<SqlDbContext, AppDbContext>(opts =>
                opts.UseNpgsql(cfg.GetConnectionString("PostgresConnection")));

            // Generic repo
            services.AddScoped(typeof(IGenericSqlRepository<>),
                               typeof(GenericRepository<>));

            // Specific product repo
            services.AddScoped<IProductRepository, ProductRepository>();

            // Unit-of-Work
            services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();

            return services;
        }
    }
}
