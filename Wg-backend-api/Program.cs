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
using Wg_backend_api.Services;
using Wg_backend_api.Data.Seeders;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Wg_backend_api.Auth;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication;

namespace Wg_backend_api.Data
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add DbContexts  
            builder.Services.AddDbContext<GlobalDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddDbContext<GameDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(connectionString);
                //.EnableSensitiveDataLogging() //Enable for debug
                //.LogTo(Console.WriteLine, LogLevel.Debug) ;
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
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "MyCookieAuth";
                options.DefaultChallengeScheme = "Google";
            })
            .AddCookie("MyCookieAuth", options =>
            {
                options.LoginPath = "/api/auth/login";
                options.Cookie.Name = "MyAppAuth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            //.AddGoogle("Google", options =>
            //{
            //    options.ClientId = ""; 
            //    options.ClientSecret = "";

            //    options.ClaimActions.MapJsonKey("picture", "picture");
            //    options.ClaimActions.MapJsonKey("locale", "locale");
            //    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

            //    options.SaveTokens = true;
            //    options.CallbackPath = "/signin-google";
            //})
            ;

            builder.Services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim(ClaimTypes.NameIdentifier)
                    .Build();
            });

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
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ISessionDataService, SessionDataService>();


            builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");


            var app = builder.Build();

            // Configure the HTTP request pipeline  
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            var corsService = app.Services.GetRequiredService<ICorsService>();
            var corsPolicyProvider = app.Services.GetRequiredService<ICorsPolicyProvider>();

            // Konfiguracja plik�w statycznych z CORS
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(app.Environment.ContentRootPath, "Resources", "Images")),
                RequestPath = "/images",
                OnPrepareResponse = ctx =>
                {
                    var policy = corsPolicyProvider.GetPolicyAsync(ctx.Context, "AllowAngular")
                        .ConfigureAwait(false)
                    .GetAwaiter().GetResult();

                    var corsResult = corsService.EvaluatePolicy(ctx.Context, policy);
                    corsService.ApplyResult(corsResult, ctx.Context.Response);
                }
            });


            app.UseSession();

            app.UseHttpsRedirection();

            app.UseStaticFiles(); // Teraz z obs�ug� CORS

            app.UseRouting(); // Jawnie dodane
            app.UseSession(); // Tutaj dodajemy middleware sesji

            app.UseCors("AllowAngular"); // Po routingu, przed autentykacj�

            app.UseAuthentication();

            app.UseMiddleware<ValidateUserIdMiddleware>();
            app.UseMiddleware<GameAccessMiddleware>();  

            app.UseAuthorization();
            app.MapControllers(); // Map controller routes  


            if (args.Contains("--global")) {
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
                }
                GameService.GenerateGlobalSchema(connectionString, Directory.GetCurrentDirectory() + "\\Migrations\\globalInitalize");
            }
            if (args.Contains("--tmp-game"))
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
                }
                GameService.GenerateNewGame(connectionString, Directory.GetCurrentDirectory() + "\\Migrations\\initate.sql", "game_1");
            }

            if (args.Contains("--seeder"))
            {
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var dbContext = services.GetRequiredService<GlobalDbContext>();
                    var seeder = new GlobalSeeder(dbContext);
                    seeder.Seed();
                }
            }


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
