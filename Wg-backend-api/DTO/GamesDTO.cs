namespace Wg_backend_api.DTO
{
    public class PlayerGamesDTO
    {
        public List<GameDTO> PlayerGames { get; set; } = [];
    }

    public class GameDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? GameCode { get; set; }

        public GameDTO(int? id, string name, string? description, string? image, string? gameCode)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Image = image;
            this.GameCode = gameCode;
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
