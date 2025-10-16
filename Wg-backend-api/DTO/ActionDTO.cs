using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.DTO
{
    public class ActionDTO
    {
        public ActionDTO() { }

        public ActionDTO(ActionDTO other)
        {
            this.Id = other.Id;
            this.NationId = other.NationId;
            this.Name = other.Name;
            this.Description = other.Description;
            this.Result = other.Result;
            this.IsSettled = other.IsSettled;
        }

        public int? Id { get; set; }

        public int NationId { get; set; }

        public string? Name { get; set; }

        public string Description { get; set; }

        public string? Result { get; set; }

        public bool IsSettled { get; set; }
    }
}
