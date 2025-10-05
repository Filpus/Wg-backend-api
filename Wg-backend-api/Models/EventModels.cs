using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wg_backend_api.Enums;

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
        [ForeignKey("EventId")]
        public Event Event { get; set; }

        [Required]
        [Column("fk_Nations")]
        public int NationId { get; set; }
        [ForeignKey("NationId")]
        public Nation Nation { get; set; }
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

        public ICollection<RelatedEvents> RelatedEvents { get; set; }
        public ICollection<Modifiers> Modifiers { get; set; }
    }


    [Table("modifiers")]
    public class Modifiers
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        [Required]
        [Column("event_id")]
        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public Event Event { get; set; }
        [Required]
        [Column("modifier_type")]
        public ModifierType modiferType { get; set; }
        [Required]
        [Column("effects")]
        public string Effects { get; set; }


    }
}
