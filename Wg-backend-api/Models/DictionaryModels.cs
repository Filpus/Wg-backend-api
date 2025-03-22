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
        [Required]
        [Column("fk_Resources")]
        public int ResourceId { get; set; }
        [Required]
        [Column("amount")]
        public int Amount { get; set; }
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
        [Required]
        [Column("fk_Resources")]
        public int ResourceId { get; set; }
        [Required]
        [Column("coefficient")]
        public int Coefficient { get; set; }
    }

}