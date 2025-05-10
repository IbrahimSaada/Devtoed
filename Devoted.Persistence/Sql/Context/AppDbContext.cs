using System.Reflection.Emit;
using Devoted.Domain.Sql.Entity;
using Devoted.GenericLibrary.GenericSql.Context;
using Microsoft.EntityFrameworkCore;

namespace Devoted.Persistence.Sql.Context
{
    public class AppDbContext : SqlDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Explicit DbSets are optional but nice for LINQ access
        public DbSet<Products> Products => Set<Products>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // apply IEntityTypeConfiguration<> in same assembly
            mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(mb);

            mb.Entity<Products>().Property(p => p.Id).ValueGeneratedOnAdd();
        }
    }
}
