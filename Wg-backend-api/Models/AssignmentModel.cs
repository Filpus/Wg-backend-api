using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.Models
{
    [Table("accessestonations")]
    public class Assignment
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Required]
        [Column("fk_nations")]
        public int NationId { get; set; }
        [ForeignKey("NationId")]
        public Nation Nation { get; set; }

        [Required]
        [Column("fk_users")]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public Player User { get; set; }

        [Required]
        [Column("dateacquired")]
        public DateTime DateAcquired { get; set; }

        [Column("isactive")]
        [Required]
        public bool IsActive { get; set; }
    }
}
