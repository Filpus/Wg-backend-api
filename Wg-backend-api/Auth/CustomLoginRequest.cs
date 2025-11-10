namespace Wg_backend_api.Auth
{
    public class CustomLoginRequest
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class RefreshRequest {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
