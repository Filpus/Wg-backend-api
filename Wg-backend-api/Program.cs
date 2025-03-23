//using Microsoft.EntityFrameworkCore;
//using Wg_backend_api.Data;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//// Rejestracja DbContext z po³¹czeniem do bazy danych PostgreSQL
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("Host=localhost;Username=postgres;Password=admin;Database=wg")));


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", builder =>
//    {
//        builder.AllowAnyOrigin()  // Zezwala na dostêp z ka¿dego Ÿród³a
//               .AllowAnyMethod()  // Zezwala na wszystkie metody HTTP
//               .AllowAnyHeader(); // Zezwala na wszystkie nag³ówki
//    });
//});


//builder.Services.AddControllers();

//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();  

//app.UseCors("AllowAll");

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

////using System;
////using System.Linq;
////using Wg_backend_api.Data;

////class Program
////{
////    static void Main(string[] args)
////    {
////        using (var context = new AppDbContext())
////        {
////            var resources = context.Resources.ToList();

////            // Sprawdzamy, czy s¹ jakieœ zasoby, a nastêpnie wypisujemy je
////            if (resources.Any())
////            {
////                Console.WriteLine("Lista zasobów:");
////                foreach (var resource in resources)
////                {
////                    Console.WriteLine($"ID: {resource.Id}, Name: {resource.Name}, IsMain: {resource.IsMain}");
////                }
////            }
////            else
////            {
////                Console.WriteLine("Brak zasobów w bazie.");
////            }
////        }
////    }
////}
//using Wg_backend_api.Data;

//class Program
//{
//    static void Main(string[] args)
//    {
//        // Tworzenie IServiceProvider (np. za pomoc¹ Dependency Injection)
//        var serviceProvider = new ServiceCollection()
//            .AddDbContext<GameDbContext>() // Rejestracja DbContext w DI
//            .BuildServiceProvider();

//        // Tworzenie instancji GameService
//        var gameService = new GameService(serviceProvider);

//        // Tworzenie instancji DatabaseInitializer
//        var initializer = new DatabaseInitializer(gameService);

//        // Inicjalizacja bazy danych (globalny schemat + dwa schematy dla gier)
//        initializer.InitializeDatabase();
//    }
//}
using System;
using Microsoft.EntityFrameworkCore;

namespace Wg_backend_api.Data
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<GameDbContext>()
                .UseNpgsql("Host=localhost;Username=postgres;Password=Filip1234;Database=wg")
                .Options;

            string connectionString = "Host=localhost;Username=postgres;Password=Filip1234;Database=wg";
            GameService.GenerateNewGame(connectionString, "C:\\Users\\FilipPuszko(272731)\\source\\repos\\Wg-backend-api\\Wg-backend-api\\Migrations\\globalInitalize", "Global");
        }
    }
}
