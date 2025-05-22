
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration;
using Wg_backend_api.Data;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Wg_backend_api.Services;

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

            // Konfiguracja plików statycznych z CORS
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
            app.UseStaticFiles(); // Teraz z obs³ug¹ CORS

            app.UseRouting(); // Jawnie dodane
            app.UseSession(); // Tutaj dodajemy middleware sesji

            app.UseCors("AllowAngular"); // Po routingu, przed autentykacj¹
            app.UseAuthentication();
            app.UseAuthorization();




            app.MapControllers(); // Map controller routes  

            app.Run();
        }
    }
}

