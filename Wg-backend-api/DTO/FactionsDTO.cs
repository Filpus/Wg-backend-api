using System.ComponentModel.DataAnnotations.Schema;
using Wg_backend_api.Models;

namespace Wg_backend_api.DTO
{
    public class FactionsDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public int Power { get; set; }

        public string Agenda { get; set; }

        public int Contentment { get; set; }
        public string Color { get; set; }
    }
}
