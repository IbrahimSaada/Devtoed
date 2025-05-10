using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devoted.Domain.Sql.Request.Base
{
    public class PaginationRequest
    {
        public int Skip { get; set; }
        public int Limit { get; set; }
    }
}
