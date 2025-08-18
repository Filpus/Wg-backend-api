using System.ComponentModel.DataAnnotations.Schema;
using Wg_backend_api.Models;

namespace Wg_backend_api.DTO
{
    public class UserDTO
    {
    }

    public class UserPathDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

}
