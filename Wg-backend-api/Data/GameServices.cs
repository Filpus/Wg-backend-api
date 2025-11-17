using Npgsql;

namespace Wg_backend_api.Data
{
    public class GameService
    {

        public static bool GenerateNewGame(string connectionString, string sqlScriptPath, string schema)
        {
            string script = File.ReadAllText(sqlScriptPath);

            script = script.Replace("game_1", schema);

            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            using var command = new NpgsqlCommand(script, connection);
            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"Migracje zostały zastosowane dla schematu: {schema}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas stosowania migracji dla schematu {schema}: {ex.Message}");
                return false;
            }

            return true;
        }

        public static void GenerateGlobalSchema(string connectionString, string sqlScriptPath)
        {
            GenerateNewGame(connectionString, sqlScriptPath, "Global");
        }
    }
}
