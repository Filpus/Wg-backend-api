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

        [ForeignKey("ReligionId")]
        public Religion Religion { get; set; }

        [Required]
        [Column("fk_cultures")]
        public int CultureId { get; set; }

        [ForeignKey("CultureId")]
        public Culture Culture { get; set; }

        public ICollection<Army> Armies { get; set; }
        public ICollection<UnitOrder> UnitOrders { get; set; }
        public ICollection<Action> Actions { get; set; }
        public Assignment Assignment { get; set; }

        public ICollection<RelatedEvents> RelatedEvents { get; set; }
        public ICollection<Faction> Factions { get; set; }
        public ICollection<Localisation> Localisations { get; set; }

        [InverseProperty("OfFeringNation")]
        public ICollection<TradeAgreement> OfferedTradeAgreements { get; set; }

        [InverseProperty("ReceivingNation")]
        public ICollection<TradeAgreement> ReceivedTradeAgreements { get; set; }
    }

}
