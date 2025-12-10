using Npgsql;
using System.Text.RegularExpressions;

public static class TestDatabaseManager
{
    private const string AdminConnection 
        = "Host=localhost;Username=postgres;Password=postgres";

    private const string TestDbName = "wg_test";

    private static string TestDbConnection =>
        $"Host=localhost;Database={TestDbName};Username=postgres;Password=postgres";

    public static string GetConnectionString() => TestDbConnection;

    public static string RecreateDatabase()
    {
        using var admin = new NpgsqlConnection(AdminConnection);
        admin.Open();

        Console.WriteLine("Terminating existing connections...");
        using (var cmd = new NpgsqlCommand($@"
            SELECT pg_terminate_backend(pid) 
            FROM pg_stat_activity 
            WHERE datname = '{TestDbName}' AND pid <> pg_backend_pid()", admin))
        {
            cmd.ExecuteNonQuery();
        }

        Console.WriteLine($"Dropping database {TestDbName} if exists...");
        using (var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS {TestDbName}", admin))
        {
            cmd.ExecuteNonQuery();
        }

        Console.WriteLine($"Creating database {TestDbName}...");
        using (var cmd = new NpgsqlCommand($"CREATE DATABASE {TestDbName}", admin))
        {
            cmd.ExecuteNonQuery();
        }

        var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Api","wg-init-db-seeder.sql"); // hehe fuszera drut
        string script = File.ReadAllText(path);
        
        script = Regex.Replace(script, @"^\\.*$", "", RegexOptions.Multiline);

        Console.WriteLine($"Executing migration script (size: {script.Length} bytes)...");
        using var connection = new NpgsqlConnection(TestDbConnection);
        connection.Open();

        using var command = new NpgsqlCommand(script, connection);
        command.CommandTimeout = 600;
        try
        {
            command.ExecuteNonQuery();
            Console.WriteLine($"✓ Database initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error initializing database: {ex.Message}");
            throw;
        }

        return TestDbConnection;
    }

    public static void DropDatabase()
    {
        using var admin = new NpgsqlConnection(AdminConnection);
        admin.Open();

        using (var cmd = new NpgsqlCommand($@"
            SELECT pg_terminate_backend(pid) 
            FROM pg_stat_activity 
            WHERE datname = '{TestDbName}' AND pid <> pg_backend_pid()", admin))
        {
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS {TestDbName}", admin))
        {
            cmd.ExecuteNonQuery();
        }
    }
}
