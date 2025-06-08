
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
                options.IdleTimeout = TimeSpan.FromDays(365);
                options.Cookie.SameSite = SameSiteMode.Lax; // Nie None dla HTTP
                options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Nie Always dla HTTP// TODO how many minutes we need?
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
                    builder.WithOrigins("https://localhost:4200", "http://localhost:4200")
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



            app.UseHttpsRedirection();

            app.UseStaticFiles(); // Teraz z obs�ug� CORS

            app.UseRouting(); // Jawnie dodane
            app.UseSession(); // Tutaj dodajemy middleware sesji

            app.UseCors("AllowAngular"); // Po routingu, przed autentykacj�

            app.UseAuthentication();

            if (!args.Contains("--no-login"))
            {
                app.UseMiddleware<ValidateUserIdMiddleware>();
                app.UseMiddleware<GameAccessMiddleware>();  
            }

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

