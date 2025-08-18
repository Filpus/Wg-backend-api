namespace Wg_backend_api.DTO
{
    public class PlayerGamesDTO
    {
        public List<GameDTO> PlayerGames { get; set; } = new();
    }

    public class GameDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }

        public GameDTO(int? id, string name, string? description, string? image)
        {
            Id = id;
            Name = name;
            Description = description;
            Image = image;
        }
    }

    public class CreateGameDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; }

        public CreateGameDTO()
        {
        }
    }
}
