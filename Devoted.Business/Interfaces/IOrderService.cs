using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devoted.Domain.Sql.Request.Order;
using Devoted.Domain.Sql.Response.Base;

namespace Devoted.Business.Interfaces
{
    public interface IOrderService
    {
        Task<BaseResponse> CreateOrderAsync(CreateOrderRequest req, CancellationToken ct);
    }
}
