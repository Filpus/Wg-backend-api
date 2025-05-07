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
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration;

namespace Wg_backend_api.Data
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContexts  
            builder.Services.AddDbContext<GlobalDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddDbContext<GameDbContext>((serviceProvider, options) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(connectionString);
            });

            // Add Scoped GameDbContextFactory  
            builder.Services.AddScoped<IGameDbContextFactory, GameDbContextFactory>();


            // Session setup
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10); // TODO how many minutes we need?
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Authentication and Authorization setup  
            builder.Services.AddAuthentication("MyCookieAuth")
                .AddCookie("MyCookieAuth", options =>
                {
                    options.LoginPath = "/api/auth/login";
                    options.Cookie.Name = "MyAppAuth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });

            builder.Services.AddAuthorization();

            // CORS setup to allow access from Angular frontend  
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                           .AllowCredentials()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            // Add Controllers (API endpoints)  
            builder.Services.AddControllers();

            // Add Swagger configuration  
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline  
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowAngular");
            app.UseSession();

            app.MapControllers(); // Map controller routes  

            app.Run();
        }
    }
}

//namespace Wg_backend_api.Data
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {

//            var builder = WebApplication.CreateBuilder(args);

//            //builder.Services.AddDbContext<GameDbContext>(options =>
//            //    options.UseNpgsql(builder.Configuration.GetConnectionString("Host=localhost;Username=postgres;Password=postgres;Database=wg")));


//            builder.Services.AddDbContext<GlobalDbContext>(options =>
//            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//            builder.Services.AddDbContext<GameDbContext>(options =>
//            {
//                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")); 
//            });

//            //builder.Services.AddDbContextFactory<GameDbContext>(options =>
//            //    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//            builder.Services.AddScoped<IGameDbContextFactory, GameDbContextFactory>();


//            builder.Services.AddAuthentication("MyCookieAuth")
//            .AddCookie("MyCookieAuth", options =>
//            {
//                options.LoginPath = "/api/auth/login";
//                options.Cookie.Name = "MyAppAuth";
//                options.Cookie.HttpOnly = true;
//                options.Cookie.SameSite = SameSiteMode.None;
//                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//            });

//            builder.Services.AddAuthorization();

//            builder.Services.AddCors(options =>
//            {
//                options.AddPolicy("AllowAngular", builder =>
//                {
//                    builder.WithOrigins("http://localhost:4200")
//                           .AllowCredentials()
//                           .AllowAnyHeader()
//                           .AllowAnyMethod();
//                });
//            });


//            //builder.Services.AddCors(options =>
//            //{
//            //    options.AddPolicy("AllowAll", builder =>
//            //    {
//            //        builder.AllowAnyOrigin()  // Zezwala na dostêp z ka¿dego Ÿród³a
//            //               .AllowAnyMethod()  // Zezwala na wszystkie metody HTTP
//            //               .AllowAnyHeader(); // Zezwala na wszystkie nag³ówki
//            //    });
//            //});


//            builder.Services.AddControllers();


//            var app = builder.Build();

//            //// Configure the HTTP request pipeline.


//            app.UseHttpsRedirection();

//            app.UseAuthentication();
//            app.UseAuthorization();
//            app.UseCors("AllowAngular");


//            //app.UseCors("AllowAll");

//            //app.UseAuthorization();

//            app.MapControllers();

//            app.Run();


//            // TO ponie¿ej by³o do tworzenia \/\/\/\/\/\/\/\/\/

//            //var options = new DbContextOptionsBuilder<GameDbContext>()
//            //    .UseNpgsql("Host=localhost;Username=postgres;Password=postgres;Database=wg")
//            //    .Options;

//            //string connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=wg";


//            //GameService.GenerateNewGame(connectionString, Directory.GetCurrentDirectory() + "\\Migrations\\initate.sql", "tmp_game");
//            //GameService.GenerateGlobalSchema(connectionString, Directory.GetCurrentDirectory() + "\\Migrations\\globalInitalize");
//        }
//    }
//}
