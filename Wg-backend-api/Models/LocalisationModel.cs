using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.Models
{
    [Table("localisations")]
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

    [Table("localisationsResources")]
    public class LocalisationResource
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key] // Oznaczenie klucza głównego
        public int? Id { get; set; }
        [Required]
        [Column("fk_localisations")]
        public int LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Localisation Location { get; set; }
        [Required]
        [Column("fk_Resources")]
        public int ResourceId { get; set; }
        [ForeignKey("ResourceId")]
        public Resource Resource { get; set; }
        [Required]
        [Column("amount")]
        public float Amount { get; set; }
    }
}
