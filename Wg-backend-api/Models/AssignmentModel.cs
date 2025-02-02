using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Wg_backend_api.Models
{
    [Table("accessestonations")]
    public class Assignment
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Column("fk_nations")]
        public int NationId { get; set; }

        [Column("fk_users")]
        public int UserId { get; set; }

        [Column("dateacquired")]
        public DateTime DateAcquired { get; set; }

        [Column("isactive")]
        [Required]
        public bool IsActive { get; set; }

    }
}
