namespace Wg_backend_api.DTO
{
    public class GamesDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string Role { get; set; }

        public GamesDTO(int? id, string name, string? description, string image, string role)
        {
            Id = id;
            Name = name;
            Description = description;
            Image = image;
            Role = role;
        }
    }
}
