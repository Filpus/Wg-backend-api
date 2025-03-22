using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.Models
{

    [Table("relatedEvents")]
    public class RelatedEvents
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Required]
        [Column("fk_Events")]
        public int EventId { get; set; }
        [Required]
        [Column("fk_Nations")]
        public int NationId { get; set; }
    }

    [Table("events")]
    public class Event
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        [Required]
        [Column("name")]
        public string Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("picture")]
        public string? Picture { get; set; }
    }


    [Table("modifiers")]
    public class Modifiers
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Required]
        [Column("fk_Events")]
        public int EventId { get; set; }

        [Required]
        [Column("modifireType")]
        public ModifireType modifireType { get; set; }

        [Column("fk_Resources")]
        public int? ResourceId { get; set; }
        [Column("fk_SocialGroups")]
        public int? SocialGroupId { get; set; }
        [Column("fk_Cultures")]
        public int? CultureId { get; set; }
        [Column("fk_Religion")]
        public int? ReligionId { get; set; }

    }
}
