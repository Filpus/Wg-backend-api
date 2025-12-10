using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Wg_backend_api.Auth;
using Wg_backend_api.Data;
using Wg_backend_api.Logic.Modifiers;
using Wg_backend_api.Logic.Modifiers.Processors;
using Wg_backend_api.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = string.Empty;
if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("DevConection");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DeploymentConection");
}

builder.Services.AddSingleton(new GameService(connectionString));

// Add DbContexts
builder.Services.AddDbContext<GlobalDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<GameDbContext>((serviceProvider, options) => options.UseNpgsql(connectionString));

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
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = "External";
})
.AddCookie("External")
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateLifetime = true,
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
    };
})
.AddGoogle("Google", options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

    options.ClaimActions.MapJsonKey("picture", "picture");
    options.ClaimActions.MapJsonKey("locale", "locale");
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

    options.SaveTokens = true;
    options.CallbackPath = "/signin-google";

    options.SignInScheme = "External";
});

builder.Services.AddAuthorization(options => options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(ClaimTypes.NameIdentifier)
        .Build());

// CORS setup to allow access from Angular frontend
builder.Services.AddCors(options => options.AddPolicy("AllowAngular", builder => builder.WithOrigins("https://localhost:4200", "http://localhost:4200", "https://localhost", "https://wargameshub.pl")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod()));

builder.Services.AddHostedService<RefreshTokenCleanupService>();

builder.Services.AddScoped<UserIdActionFilter>();

// Add Controllers (API endpoints)
builder.Services.AddControllers(config => 
{
    config.Filters.Add<UserIdActionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
    options.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = true;
});

// Add Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISessionDataService, SessionDataService>();
builder.Services.AddScoped<ResourceChangeProcessor>();
builder.Services.AddScoped<PopulationHappinessProcessor>();
builder.Services.AddScoped<PopulationResourceProductionProcessor>();
builder.Services.AddScoped<PopulationResourceUsageProcessor>();
builder.Services.AddScoped<PopulationVolunteerProcessor>();
builder.Services.AddScoped<FactionPowerProcessor>();
builder.Services.AddScoped<FactionContentmentProcessor>();

// rejestracja factory
builder.Services.AddScoped<ModifierProcessorFactory>();

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

// app.UseHttpsRedirection();

//app.UseStaticFiles(); // Teraz z obs�ug� CORS

app.UseRouting(); // Jawnie dodane
app.UseSession(); // Tutaj dodajemy middleware sesji

app.UseCors("AllowAngular"); // Po routingu, przed autentykacj�

app.UseAuthentication();

if (!args.Contains("--no-login"))
{
    // app.UseMiddleware<ValidateUserIdMiddleware>();
}

app.UseMiddleware<GameAccessMiddleware>();
app.UseAuthorization();

app.MapControllers(); // Map controller routes

app.Run();

public partial class Program { }