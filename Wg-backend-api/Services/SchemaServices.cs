namespace Wg_backend_api.Services
{
    public interface ISessionDataService
    {
        public string GetSchema();
        public void SetSchema(string schema);

        public string? GetNation();
        public void SetNation(string nation);

        public string? GetRole();
        public void SetRole(string role);

        public string? GetUserIdItems();
        public void SetUserIdItems(string id);
    }

    public class SessionDataService : ISessionDataService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionDataService(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        public void SetSchema(string schema)
        {
            if (this._httpContextAccessor.HttpContext?.Session == null)
            {
                throw new InvalidOperationException("Sesja nie jest dostępna");
            }

            this._httpContextAccessor.HttpContext.Session.SetString("Schema", schema);
        }

        public string GetSchema()
        {
            return this._httpContextAccessor.HttpContext?.Session.GetString("Schema");
        }

        public void SetNation(string nation)
        {
            if (this._httpContextAccessor.HttpContext?.Session == null)
            {
                throw new InvalidOperationException("Sesja nie jest dostępna");
            }

            this._httpContextAccessor.HttpContext.Session.SetString("Nation", nation);
        }

        public string? GetNation()
        {
            return this._httpContextAccessor.HttpContext?.Session.GetString("Nation");
        }

        public void SetRole(string role)
        {
            if (this._httpContextAccessor.HttpContext?.Session == null)
            {
                throw new InvalidOperationException("Sesja nie jest dostępna");
            }

            this._httpContextAccessor.HttpContext.Session.SetString("Role", role);
        }

        public string? GetRole()
        {
            return this._httpContextAccessor.HttpContext?.Session.GetString("Role");
        }

        public string? GetUserIdItems()
        {
            return this._httpContextAccessor.HttpContext?.Items["UserId"]?.ToString();
        }

        public void SetUserIdItems(string id)
        {
            this._httpContextAccessor.HttpContext.Items["UserId"] = id;
        }
    }

}
