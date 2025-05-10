using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devoted.Domain.Sql.Dto
{
    public record BulkUpdateDto(long Id, string Name, decimal Price);
}
