namespace Wg_backend_api.Services
{
    public interface ISessionDataService
    {
        string GetSchema();
        void SetSchema(string schema);

        string? GetNation();
        void SetNation(string nation);
    }

    public class SessionDataService : ISessionDataService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionDataService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public void SetSchema(string schema)
        {
            if (_httpContextAccessor.HttpContext?.Session == null)
            {
                throw new InvalidOperationException("Sesja nie jest dostępna");
            }

            _httpContextAccessor.HttpContext.Session.SetString("Schema", schema);
        }
        public string GetSchema()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("Schema");
        }

        public void SetNation(string nation)
        {
            if (_httpContextAccessor.HttpContext?.Session == null)
            {
                throw new InvalidOperationException("Sesja nie jest dostępna");
            }

            _httpContextAccessor.HttpContext.Session.SetString("Nation", nation);
        }
        public string? GetNation()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("Nation");
        }
    }

}
