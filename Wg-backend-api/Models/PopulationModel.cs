using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Wg_backend_api.Models
{
    [Table("populations")]
    public class Population
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key] // Oznaczenie klucza głównego
        public int? Id { get; set; }

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

        [Required]
        [Column("fk_socialgroups")]
        public int SocialGroupId { get; set; }
        [ForeignKey("SocialGroupId")]
        public SocialGroup SocialGroup { get; set; }

        [Required]
        [Column("fk_locations")]
        public int LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Localisation Location { get; set; }

        [Required]
        [Column("happiness")]
        public float Happiness { get; set; }

        [Required]
        [Column("volunteers")]
        public int Volunteers { get; set; }


        public ICollection<PopulationUsedResource> PopulationUsedResources { get; set; }
        public ICollection<PopulationProductionShare> PopulationProductionShares { get; set; }
    }


    [Table("populationproductionshares")]
    public class PopulationProductionShare
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int? Id { get; set; }

        [Required]
        [Column("fk_population")]
        public int PopulationId { get; set; }
        [ForeignKey("PopulationId")]
        public Population Population { get; set; }

        [Required]
        [Column("fk_resources")]
        public int ResourcesId { get; set; }
        [ForeignKey("ResourcesId")]
        public Resource Resources { get; set; }

        [Required]
        [Column("coefficient")]
        public float Coefficient { get; set; }


    }

    [Table("populationusedresource")]
    public class PopulationUsedResource
     {
         [Column("id")]
         [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
         [Key]
         public int? Id { get; set; }

         [Required]
         [Column("fk_population")]
         public int PopulationId { get; set; }
         [ForeignKey("PopulationId")]
         public Population Population { get; set; }

         [Required]
         [Column("fk_resources")]
         public int ResourcesId { get; set; }
         [ForeignKey("ResourcesId")]
         public Resource Resources { get; set; }

         [Required]
         [Column("amount")]
         public float Amount { get; set; }
     }
}
