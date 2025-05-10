using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devoted.Domain.Sql.Entity.Base
{
    public class BaseSqlEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public long Id { get; set; }

        [Column("recordguid")]
        public Guid RecordGuid { get; set; } = Guid.NewGuid();

        [Column("isdeleted")]
        public bool IsDeleted { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }
    }
}
