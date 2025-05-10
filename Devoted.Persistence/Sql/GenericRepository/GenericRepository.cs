using Devoted.Domain.Sql.Entity.Base;
using Devoted.GenericLibrary.GenericSql.Interfaces;
using Devoted.GenericLibrary.GenericSql.Repositories;
using Devoted.Persistence.Sql.Context;

namespace Devoted.Persistence.Sql.GenericRepository
{
    public class GenericRepository<T> : GenericSqlRepository<T>, IGenericSqlRepository<T>
        where T : BaseSqlEntity
    {
        public GenericRepository(AppDbContext ctx) : base(ctx) { }
    }
}

