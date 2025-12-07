using System.Text.RegularExpressions;
using Npgsql;

namespace Wg_backend_api.Data
{
    public class GameService
    {
        private readonly string _connectionString;

        public GameService(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public bool GenerateNewGame(string sqlScriptPath, string schema)
        {
            string script = File.ReadAllText(sqlScriptPath);

            script = Regex.Replace(script, @"^\\.*$", "", RegexOptions.Multiline);

            script = script.Replace("game_1", schema);

            using var connection = new NpgsqlConnection(this._connectionString);
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

        public void GenerateGlobalSchema(string sqlScriptPath)
        {
            this.GenerateNewGame(sqlScriptPath, "Global");
        }

        public bool DeleteGameSchema(string schema)
        {
            using var connection = new NpgsqlConnection(this._connectionString);
            connection.Open();

            string dropSchemaSql = $"DROP SCHEMA IF EXISTS {schema} CASCADE;";

            using var command = new NpgsqlCommand(dropSchemaSql, connection);
            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"Schema {schema} has been deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting schema {schema}: {ex.Message}");
                return false;
            }

            return true;
        }
    }
}
