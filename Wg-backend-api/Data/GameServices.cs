using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

using System.Reflection;
using Npgsql;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Wg_backend_api.Data
{
    public class GameService
    {


        // Podaj adres do initate.sql
        public static bool GenerateNewGame(string connectionString, string sqlScriptPath, string schema)
        {
            string script = File.ReadAllText(sqlScriptPath);

            // Zamień domyślny schemat na docelowy schemat
            script = script.Replace("default_schema", schema);

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

//      Podaj adres pliku globalInitalize
        public static void GenerateGlobalSchema(string connectionString, string sqlScriptPath)
        {
            GenerateNewGame(connectionString, sqlScriptPath, "Global");
        }
    }

    //public class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
    //{
    //    public GameDbContext CreateDbContext(string[] args)
    //    {
    //        // Konfiguracja opcji DbContext
    //        var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
    //        optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=postgres;Database=wg");

    //        // Domyślny schemat na potrzeby design-time
    //        string defaultSchema = "default_schema";

    //        // Utworzenie instancji GameDbContext
    //        return new GameDbContext(optionsBuilder.Options, defaultSchema);
    //    }
    //}


    //public class GlobalDbContextFactory : IDesignTimeDbContextFactory<GlobalDbContext>
    //{
    //    public GlobalDbContext CreateDbContext(string[] args)
    //    {
    //        // Ładowanie konfiguracji z pliku appsettings.json
    //        IConfigurationRoot configuration = new ConfigurationBuilder()
    //            .SetBasePath(Directory.GetCurrentDirectory())
    //            .AddJsonFile("appsettings.json")
    //            .Build();

    //        // Pobranie connection stringa z konfiguracji
    //        var connectionString = configuration.GetConnectionString("Host=localhost;Username=postgres;Password=postgres;Database=wg");

    //        // Konfiguracja DbContextOptions
    //        var optionsBuilder = new DbContextOptionsBuilder<GlobalDbContext>();
    //        optionsBuilder.UseNpgsql(connectionString); // Użyj UseSqlServer dla SQL Server

    //        // Utworzenie instancji GlobalDbContext
    //        return new GlobalDbContext(optionsBuilder.Options);
    //    }
    //}



}