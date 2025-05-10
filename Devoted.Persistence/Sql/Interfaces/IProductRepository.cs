using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devoted.Domain.Sql.Entity;
using Devoted.GenericLibrary.GenericSql.Interfaces;

namespace Devoted.Persistence.Sql.Interfaces
{
    public interface IProductRepository : IGenericSqlRepository<Products>
    {
    }
}
