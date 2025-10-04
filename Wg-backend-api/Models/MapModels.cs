using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.Models
{
    [Table("map")]
    public class Map
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("mapLocation")]
        public string MapLocation { get; set; }

        [Required]
        [Column("mapIconLocation")]
        public string MapIconLocation { get; set; }

        public ICollection<MapAccess> MapAccesses { get; set; }
    }

    [Table("mapAccess")]
    public class MapAccess
    {
        [Column("fk_Nations")]
        [Key]
        public int NationId { get; set; }

        [ForeignKey("NationId")]
        public Nation Nation { get; set; }

        [Column("fk_Maps")]
        [Key]
        public int MapId { get; set; }

        [ForeignKey("MapId")]
        public Map Map { get; set; }
    }
}
