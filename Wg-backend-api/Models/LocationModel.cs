using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Wg_backend_api.Models
{
    [Table("locations")] 
public class Localisation
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
        [Column("size")]
        public int Size { get; set; }

        [Required]
        [Column("fortifications")]
        public int Fortification { get; set; }

        [Required]
        [Column("fk_nations")]
        public int NationId { get; set; }

        [ForeignKey("NationId")]
        public Nation Nation { get; set; }

        // Powiązanie z populacjami  
        public ICollection<Population> Populations { get; set; }

        // Powiązanie z armiami  
        public ICollection<Army> Armies { get; set; }
        public ICollection<LocalisationResource> LocalisationResources { get; set; } 
    }

    [Table("locationsResources")]
    public class LocalisationResource
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key] // Oznaczenie klucza głównego
        public int? Id { get; set; }
        [Required]
        [Column("fk_locations")]
        public int LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Localisation Location { get; set; }
        [Required]
        [Column("fk_resources")]
        public int ResourceId { get; set; }
        [ForeignKey("ResourceId")]
        public Resource Resource { get; set; }
        [Required]
        [Column("amount")]
        public int Amount { get; set; }
    }
}
