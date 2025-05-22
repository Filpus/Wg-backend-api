namespace Wg_backend_api.Services
{
    public interface ISessionDataService
    {
        string GetSchema();
        void SetSchema(string schema);

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
    }

}
