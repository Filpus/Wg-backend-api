using Xunit;

namespace Tests.Api;
public class DatabaseFixture : IDisposable
{
    public string ConnectionString { get; }

    public DatabaseFixture()
    {
        ConnectionString = TestDatabaseManager.RecreateDatabase();
    }

    public void Dispose()
    {
        TestDatabaseManager.DropDatabase();
    }
}

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
