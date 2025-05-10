using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devoted.Domain.Sql.Request.Order
{
    public record CreateOrderRequest(long ProductId, int Quantity);
}
