using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devoted.Domain.Sql.Dto;

namespace Devoted.Business.Interfaces
{
    public interface IProductClient
    {
        Task<ProductDto> GetByIdAsync(long id, CancellationToken ct);
    }
}
