using Wg_backend_api.Services;

public class TestSessionDataService : ISessionDataService
{
    private string _schema;
    private string _nation;
    private string? _userId;
    private string? _role;

    public TestSessionDataService(string schema, string nation, string role = "Player")
    {
        _schema = schema;
        _nation = nation;
        _role = role;
    }

    public string GetSchema() => _schema;
    public string GetNation() => _nation;

    public string? GetRole() => _role;
    public void SetSchema(string schema)
    {
        this._schema = schema;
    }
    public void SetNation(string nation)
    {
        this._nation = nation;
    }
    public void SetRole(string role)
    {
        this._role = role;
    }

    public string? GetUserIdItems() => _userId;

    public void SetUserIdItems(string id)
    {
        this._userId = id;
    }
}
