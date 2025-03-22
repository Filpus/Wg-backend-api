using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.Models
{
    [Table("actions")]
    public class Action
    {
       
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("fk_Nations")]
        public int NationId { get; set; }


        [Column("name")]
        public string? Name { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; }

        [Column("result")]
        public string? Result { get; set; }

        [Required]
        [Column("isSettled")]
        public bool IsSettled { get; set; }
    }
}
