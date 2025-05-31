using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Wg_backend_api.Models
{
    [Table("users")]
    public class User
    {

        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key] // Oznaczenie klucza głównego
        public int? Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [Column("password")]
        public string Password { get; set; }
        [Required]
        [Column("issso")]
        public bool IsSSO { get; set; }
        [Column("image")]
        public string? Image { get; set; }

        [Required]
        [Column("isarchived")]
        public bool IsArchived { get; set; }

        public ICollection<Game> OwnedGames { get; set; }

        public ICollection<GameAccess> GameAccesses { get; set; }
    }


    [Table("players")]
    public class Player
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key] // Oznaczenie klucza głównego
        public int? Id { get; set; }

        [Required]
        [Column("fk_User")]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [Column("playerType")]
        public UserRole Role { get; set; }

        public Assignment Assignment { get; set; }

    }
}
