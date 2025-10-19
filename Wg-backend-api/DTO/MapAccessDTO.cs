namespace Wg_backend_api.DTO
{
    public class MapAccessCreateDTO
    {
        public int NationId { get; set; }
        public int MapId { get; set; }
    }

    public class MapAccessInfoDTO
    {
        public int NationId { get; set; }
        public int MapId { get; set; }
        public string NationName { get; set; }
        public string NationImage { get; set; }
        public string MapName { get; set; }
    }
}
