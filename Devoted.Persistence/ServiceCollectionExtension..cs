
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
            this IServiceCollection services, IConfiguration cfg)
        {
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(cfg.GetConnectionString("PostgresConnection")));

            // Bind Core generic repository to our adapter
            services.AddScoped(typeof(IGenericSqlRepository<>),
                               typeof(GenericRepository<>));

            services.AddScoped<IAppUnitOfWork, AppUnitOfWork>();

            return services;
        }
    }
}
