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

        [Column("size")]
        public int Size { get; set; }

        [Column("fortifications")]
        public int Fortification { get; set; }

        [Column("fk_nations")]
        public int NationId { get; set; }

    }


}