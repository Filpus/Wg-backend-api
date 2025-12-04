namespace Wg_backend_api.DTO
{
    public class UserDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsSSO { get; set; }
        public string? Image { get; set; }
    }

    public class UserPathDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class ChangeEmailDTO
    {
        public string NewEmail { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordDTO
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
