namespace Wg_backend_api.Controllers.GlobalControllers
{
    public static class ValidateUserData
    {
        public static bool isValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool isValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length <= 3 || username.Length > 40)
            {
                return false;
            }

            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                {
                    return false;
                }
            }

            return true;
        }

        public static bool isValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length <= 5 || password.Length > 255)
            {
                return false;
            }

            return true;
        }

    }
}
