using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devoted.Domain.Sql.Entity.Base;

namespace Devoted.Domain.Sql.Entity
{
    [Table("Products")]
    public class Products : BaseSqlEntity
    {
        [Column("name")]
        public string Name { get; set; } = default!;

        [Column("price")]
        public decimal Price { get; set; }
    }
}
