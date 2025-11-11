using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wg_backend_api.Enums;

namespace Wg_backend_api.Models
{
    [Table("wantedresources")]
    public class WantedResource
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Required]
        [Column("fk_resource")]
        public int ResourceId { get; set; }

        [ForeignKey("ResourceId")]
        public Resource Resource { get; set; }

        [Required]
        [Column("fk_tradeagreement")]
        public int TradeAgreementId { get; set; }

        [ForeignKey("TradeAgreementId")]
        public TradeAgreement TradeAgreement { get; set; }

        [Required]
        [Column("amount")]
        public float Amount { get; set; }
    }
    [Table("offeredresources")]
    public class OfferedResource
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Required]
        [Column("fk_resource")]
        public int ResourceId { get; set; }

        [ForeignKey("ResourceId")]
        public Resource Resource { get; set; }

        [Required]
        [Column("fk_tradeagreement")]
        public int TradeAgreementId { get; set; }

        [ForeignKey("TradeAgreementId")]
        public TradeAgreement TradeAgreement { get; set; }

        [Required]
        [Column("quantity")]
        public float Quantity { get; set; }
    }
    [Table("tradeagreements")]
    public class TradeAgreement
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Required]
        [Column("fk_nationoffering")]
        public int OfferingNationId { get; set; }

        [ForeignKey("OfferingNationId")]
        public Nation OfferingNation { get; set; }

        [Required]
        [Column("fk_nationreceiving")]
        public int ReceivingNationId { get; set; }

        [ForeignKey("ReceivingNationId")]
        public Nation ReceivingNation { get; set; }

        [Required]
        [Column("status")]
        public TradeStatus Status { get; set; }

        [Required]
        [Column("duration")]
        public int Duration { get; set; }

        [Required]
        [Column("description")]
        public string Description { get; set; } = "";

        public ICollection<OfferedResource> OfferedResources { get; set; }
        public ICollection<WantedResource> WantedResources { get; set; }
    }
}
