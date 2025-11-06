namespace Wg_backend_api.DTO
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Wg_backend_api.Models;

    public class FactionDTO
    {
        public int? Id { get; set; }

        public int? NationId { get; set; }

        public string Name { get; set; }

        public int Power { get; set; }

        public string Agenda { get; set; }

        public int Contentment { get; set; }

        public string Color { get; set; }

        public string? Description { get; set; } = string.Empty;
    }
}
