namespace Wg_backend_api.DTO
{
    public class NationBaseInfoDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; }
    }

    public class NationDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public int ReligionId { get; set; }

        public int CultureId { get; set; }

        public string? Flag { get; set; }

        public string Color { get; set; }
    }

    public class NationWithOwnerDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string? Flag { get; set; }

        public string Color { get; set; }

        public string? OwnerName { get; set; }
    }

    public class NationDetailedDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public ReligionDTO Religion { get; set; }

        public CultureDTO Culture { get; set; }

        public string? Flag { get; set; }

        public string Color { get; set; }

        public string? OwnerName { get; set; }
    }

    public class PostNationDTO
    {
        public string Name { get; set; }

        public int Religion { get; set; }

        public int Culture { get; set; }

        public IFormFile? Flag { get; set; }

        public string Color { get; set; }
    }

    public class PatchNationDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int? Religion { get; set; }

        public int? Culture { get; set; }

        public IFormFile? Flag { get; set; }

        public string? Color { get; set; }
    }
}