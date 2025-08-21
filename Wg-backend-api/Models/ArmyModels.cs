using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.Models
{

    [Table("unitTypes")]
    public class UnitType
    {
        [Required]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Required]
        [Column("name")]
        public string Name { get; set; }
        [Required]
        [Column("description")]
        public string Description { get; set; }
        [Required]
        [Column("melee")]
        public int Melee { get; set; }
        [Required]
        [Column("range")]
        public int Range { get; set; }
        [Required]
        [Column("defense")]
        public int Defense { get; set; }
        [Required]
        [Column("speed")]
        public int Speed { get; set; }
        [Required]
        [Column("morale")]
        public int Morale { get; set; }
        [Required]
        [Column("volunteersNeeded")]
        public int VolunteersNeeded { get; set; }
        [Required]
        [Column("isNaval")]
        public bool IsNaval { get; set; }

        public ICollection<ProductionCost> ProductionCosts { get; set; }
        public ICollection<MaintenaceCosts> MaintenaceCosts { get; set; }

    }


    [Table("accessToUnits")]
    public class AccessToUnit
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Required]
        [Column("fk_Nation")]
        public int NationId { get; set; }
        [ForeignKey("NationId")]
        public Nation Nation { get; set; }
        [Required]
        [Column("fk_UnitTypes")]
        public int UnitTypeId { get; set; }
        [ForeignKey("UnitTypeId")]
        public UnitType UnitType { get; set; }
    }

    [Table("unitOrders")]
    public class UnitOrder
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Required]
        [Column("fk_UnitTypes")]
        public int UnitTypeId { get; set; }
        [ForeignKey("UnitTypeId")]
        public UnitType UnitType { get; set; }
        [Required]
        [Column("fk_Nations")]
        public int NationId { get; set; }
        [ForeignKey("NationId")]
        public Nation Nation { get; set; }
        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }
    }

    [Table("productionCost")]
    public class ProductionCost
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Required]
        [Column("fk_UnitTypes")]
        public int UnitTypeId { get; set; }
        [ForeignKey("UnitTypeId")]
        public UnitType UnitType { get; set; }
        [Required]
        [Column("fk_Resources")]
        public int ResourceId { get; set; }
        [ForeignKey("ResourceId")]
        public Resource Resource { get; set; }
        [Required]
        [Column("amount")]
        public float Amount { get; set; }
    }

    [Table("maintenanceCosts")]
    public class MaintenaceCosts
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Required]
        [Column("fk_UnitTypes")]
        public int UnitTypeId { get; set; }
        [ForeignKey("UnitTypeId")]
        public UnitType UnitType { get; set; }
        [Required]
        [Column("fk_Resources")]
        public int ResourceId { get; set; }
        [ForeignKey("ResourceId")]
        public Resource Resource { get; set; }
        [Required]
        [Column("amount")]
        public float Amount { get; set; }
    }

    [Table("troops")]
    public class Troop
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Required]
        [Column("fk_UnitTypes")]
        public int UnitTypeId { get; set; }
        [ForeignKey("UnitTypeId")]
        public UnitType UnitType { get; set; }
        [Required]
        [Column("fk_Armies")]
        public int ArmyId { get; set; }
        [ForeignKey("ArmyId")]
        public Army Army { get; set; }
        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }
    }

    [Table("armies")]
    public class Army
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }
        [Required]
        [Column("name")]
        public string Name { get; set; }
        [Required]
        [Column("fk_Nations")]
        public int NationId { get; set; }
        [ForeignKey("NationId")]
        public Nation Nation { get; set; }
        [Required]
        [Column("fk_Locations")]
        public int LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Localisation Location { get; set; }

        [Required]
        [Column("is_naval")]
        public bool IsNaval { get; set; }

        public ICollection<Troop> Troops { get; set; }
    }
}
