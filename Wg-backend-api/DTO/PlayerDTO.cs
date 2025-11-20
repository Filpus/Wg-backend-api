using Wg_backend_api.Models;

namespace Wg_backend_api.DTO
{
    public class PlayerDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public UserRole Role { get; set; }
    }

    public class PlayerWithNationDTO : PlayerDTO
    {
        public NationBaseInfoDTO? Nation { get; set; }
    }
}
