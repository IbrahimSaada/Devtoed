using Microsoft.EntityFrameworkCore;

namespace Devoted.GenericLibrary.GenericSql.Context
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions options) : base(options) { }

        public new DbSet<T> Set<T>() where T : class => base.Set<T>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.ApplyConfigurationsFromAssembly(typeof(SqlDbContext).Assembly);
            base.OnModelCreating(mb);
        }
    }
}