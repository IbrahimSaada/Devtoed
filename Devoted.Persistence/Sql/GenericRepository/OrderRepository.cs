using Devoted.Domain.Sql.Entity;
using Devoted.GenericLibrary.GenericSql.Context;
using Devoted.GenericLibrary.GenericSql.Repositories;
using Devoted.Persistence.Sql.Interfaces;

namespace Devoted.Persistence.Sql.GenericRepository
{
    public class OrderRepository : GenericSqlRepository<Orders>, IOrderRepository
    {
        public OrderRepository(SqlDbContext ctx) : base(ctx) { }
    }
}
