using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devoted.Domain.Sql.Request.Product
{
    public record UpdateProductRequest(string Name, decimal Price);
}
