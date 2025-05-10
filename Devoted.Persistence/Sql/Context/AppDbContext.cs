using Devoted.GenericLibrary.GenericSql.Context;
using Microsoft.EntityFrameworkCore;

namespace Devoted.Persistence.Sql.Context
{
    public class AppDbContext : SqlDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }


        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(mb);

            
        }
    }
}
