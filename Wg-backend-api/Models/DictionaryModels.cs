using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.Models
{
    [Table("resources")]
    public class Resource
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key] // Oznaczenie klucza głównego
        public int? Id { get; set; }

        [Column("name")]
        [Required] // Pole wymagane
        [MaxLength(255)] // Opcjonalne ograniczenie długości
        public string Name { get; set; }

        [Required]
        [Column("ismain")]
        public bool IsMain { get; set; } // Typ logiczny
        [Column("icon")]
        public string? Icon { get; set; }

        public ICollection<ProductionCost> ProductionCosts { get; set; }
        public ICollection<MaintenaceCosts> MaintenaceCosts { get; set; }
        public ICollection<UsedResource> UsedResources { get; set; }
        public ICollection<ProductionShare> ProductionShares { get; set; }
        public ICollection<PopulationUsedResource> PopulationUsedResources { get; set; }
        public ICollection<PopulationProductionShare> PopulationProductionShares { get; set; }
        public ICollection<OfferedResource> OfferedResources { get; set; }
        public ICollection<WantedResource> WantedResources { get; set; }
        public ICollection<OwnedResouerce> OwnedResouerces { get; set; }

    }

    [Table("cultures")]
    public class Culture
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }



        public ICollection<Population> Populations { get; set; }
    }
    [Table("religions")]
    public class Religion
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(25)]
        public string Name { get; set; }
        [Column("icon")]
        public string? Icon { get; set; }


        public ICollection<Population> Populations { get; set; }
    }
    [Table("socialgroups")]
    public class SocialGroup
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column("basehappiness")]
        [Required]
        public float BaseHappiness { get; set; } // Typ float do przechowywania wartości zmiennoprzecinkowych

        [Column("volunteers")]
        [Required]
        public int Volunteers { get; set; } // Typ int do przechowywania liczby

        [Column("icon")]
        public string? Icon { get; set; }

        public ICollection<UsedResource> UsedResources { get; set; }
        public ICollection<ProductionShare> ProductionShares { get; set; }
        public ICollection<Population> Populations { get; set; }
    }


    [Table("usedResources")]
    public class UsedResource
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Required]
        [Column("fk_SocialGroups")]
        public int SocialGroupId { get; set; }

        [ForeignKey("SocialGroupId")]
        public SocialGroup SocialGroup { get; set; }

        [Required]
        [Column("fk_Resources")]
        public int ResourceId { get; set; }

        [ForeignKey("ResourceId")]
        public Resource Resource { get; set; }

        [Required]
        [Column("amount")]
        public float Amount { get; set; }
    }

    [Table("productionShares")]
    public class ProductionShare
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Required]
        [Column("fk_SocialGroups")]
        public int SocialGroupId { get; set; }

        [ForeignKey("SocialGroupId")]
        public SocialGroup SocialGroup { get; set; }

        [Required]
        [Column("fk_Resources")]
        public int ResourceId { get; set; }

        [ForeignKey("ResourceId")]
        public Resource Resource { get; set; }

        [Required]
        [Column("coefficient")]
        public float Coefficient { get; set; }
    }


    [Table("ownedResources")]
    public class OwnedResouerce()
    {
        [Column("id")]
        public int? Id { get; set; }

        [Column("fk_nation")]
        public int NationId { get; set; }

        [ForeignKey ("NationId")]
        public Nation Nation { get; set; }

        [Column("fk_resource")]
        public int ResourceId { get; set; }

        [ForeignKey("ResourceId")]
        public Resource Resource { get; set; }

        [Column("amount")]
        public float Amount { get; set; }
    }
}