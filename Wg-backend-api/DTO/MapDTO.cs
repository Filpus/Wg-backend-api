namespace Wg_backend_api.DTO
{
    public class MapDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string MapLocation { get; set; }

        public string MapIconLocation { get; set; }
    }

    public class MapCreateDTO
    {
        public int? id { get; set; }

        public string? Name { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
