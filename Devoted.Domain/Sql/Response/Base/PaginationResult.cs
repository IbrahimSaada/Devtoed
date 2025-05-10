using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devoted.Domain.Sql.Response.Base
{
    public class PaginationResult
    {
        public long TotalDocuments { get; set; }
        public long DocumentsLeft { get; set; }
    }
}
