namespace Wg_backend_api.Auth
{
    public class CustomLoginRequest
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshRequest {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
