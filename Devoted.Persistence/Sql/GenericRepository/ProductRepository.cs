using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devoted.Domain.Sql.Entity;
using Devoted.GenericLibrary.GenericSql.Context;
using Devoted.GenericLibrary.GenericSql.Repositories;
using Devoted.Persistence.Sql.Interfaces;

namespace Devoted.Persistence.Sql.GenericRepository
{
    public class ProductRepository : GenericSqlRepository<Products>, IProductRepository
    {
        public ProductRepository(SqlDbContext ctx) : base(ctx) { }
    }
}
