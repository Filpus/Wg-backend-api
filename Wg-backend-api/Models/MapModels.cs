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

    }


    [Table("mapAccess")]
    public class MapAccess
    {
        [Column("fk_Users")]
        [Key]
        public int UserId { get; set; }
        [Column("fk_Maps")]
        [Key]
        public int MapId { get; set; }

    }
}
