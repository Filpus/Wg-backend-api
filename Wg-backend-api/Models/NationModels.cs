using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Wg_backend_api.Models
{
    [Table("nations")]
    public class Nation
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Column("name")]
        [Required]
        public string Name { get; set; }
        [Required]
        [Column("fk_religions")]
        public int ReligionId { get; set; }
        [Required]
        [Column("fk_cultures")]
        public int CultureId { get; set; }

    }

}
