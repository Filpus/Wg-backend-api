using Wg_backend_api.Models;

namespace Wg_backend_api.DTO
{
    public class PlayerInfoDTO
    {
        public string Name { get; set; }
        public string? NationName { get; set; }
        public  UserRole userRole { get; set; }
    }
}
