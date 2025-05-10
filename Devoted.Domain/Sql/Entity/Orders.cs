using System.ComponentModel.DataAnnotations.Schema;
using Devoted.Domain.Sql.Entity.Base;

namespace Devoted.Domain.Sql.Entity
{
    [Table("Orders")]
    public class Orders : BaseSqlEntity
    {
        [Column("productid")]
        public long ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("total")]
        public decimal Total { get; set; }
    }
}
