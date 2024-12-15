﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Wg_backend_api.Models
{
    [Table("wantedresources")]

    public class WantedResource
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Column("fk_resource")]
        public int ResourceId { get; set; }
        [Column("fk_tradeagreement")]
        public int TradeAgreementId { get; set; }
        [Column("amount")]
        public int Amount { get; set; }
    }
    [Table("offeredresources")]
    public class OfferedResource
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Column("fk_resource")]
        public int ResourceId { get; set; }
        [Column("fk_tradeagreement")]
        public int TradeAgreementId { get; set; }
        [Column("quantity")]
        public int Quantity { get; set; }
    }
    [Table("tradeagreements")]
    public class TradeAgreement
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Column("fk_nationoffering")]
        public int OferingNationId { get; set; }
        [Column("fk_nationreceiving")]
        public int ReceivingNationId { get; set; }
        [Column("isaccepted")]
        public bool isAccepted { get; set; }
        [Column("duration")]
        public int Duration { get; set; }

    }
}
