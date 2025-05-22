using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.Models
{
    [Table("games")]
    public class Game
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key] // Oznaczenie klucza głównego
        public int Id { get; set; }
        [Required]
        [Column("name")]
        public string Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("image")]
        public string? Image { get; set; }
        [Column("ownerId")]
        [Required]
        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public ICollection<GameAccess> GameAccesses { get; set; }
    }

    [Table("gameaccess")]
    public class GameAccess
    {
        [Column("fk_Users")]
        [Required]
        [Key]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Column("fk_Games")]
        [Required]
        [Key]
        public int GameId { get; set; }
        [ForeignKey("GameId")]
        public Game Game { get; set; }

        [Required]
        [Column("accessType")]
        public UserRole Role { get; set; }

        [Required]
        [Column("isArchived")]
        public bool IsArchived { get; set; }
    }
}
