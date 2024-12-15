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
        public string Name { get; set; }
        [Column("fk_religion")]
        public int ReligionId { get; set; }
        [Column("fk_culture")]
        public int CultureId { get; set; }

    }

}
