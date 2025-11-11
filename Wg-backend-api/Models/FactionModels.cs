using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.Models
{
    [Table("factions")]
    public class Faction
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Column("name")]
        [Required]
        public string Name { get; set; }

        [Required]
        [Column("fk_Nations")]
        public int NationId { get; set; }

        [ForeignKey("NationId")]
        public Nation Nation { get; set; }

        [Required]
        [Column("power")]
        public int Power { get; set; }

        [Required]
        [Column("agenda")]
        public string Agenda { get; set; }

        [Required]
        [Column("contentment")]
        public int Contentment { get; set; }

        [Required]
        [Column("color")]
        public string Color { get; set; }

        [Column("description")]
        public string? Description { get; set; }

    }
}
