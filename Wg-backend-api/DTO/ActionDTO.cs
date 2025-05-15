using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.DTO
{
    public class ActionDTO
    {
        public int? Id { get; set; }


        public int NationId { get; set; }


        public string? Name { get; set; }


        public string Description { get; set; }


        public string? Result { get; set; }


        public bool IsSettled { get; set; }
    }
}
