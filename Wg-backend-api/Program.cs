using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Rejestracja DbContext z po³¹czeniem do bazy danych PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Host=localhost;Username=postgres;Password=Filip1234;Database=wg")));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()  // Zezwala na dostêp z ka¿dego Ÿród³a
               .AllowAnyMethod()  // Zezwala na wszystkie metody HTTP
               .AllowAnyHeader(); // Zezwala na wszystkie nag³ówki
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

//using System;
//using System.Linq;
//using Wg_backend_api.Data;

//class Program
//{
//    static void Main(string[] args)
//    {
//        using (var context = new AppDbContext())
//        {
//            var resources = context.Resources.ToList();

//            // Sprawdzamy, czy s¹ jakieœ zasoby, a nastêpnie wypisujemy je
//            if (resources.Any())
//            {
//                Console.WriteLine("Lista zasobów:");
//                foreach (var resource in resources)
//                {
//                    Console.WriteLine($"ID: {resource.Id}, Name: {resource.Name}, IsMain: {resource.IsMain}");
//                }
//            }
//            else
//            {
//                Console.WriteLine("Brak zasobów w bazie.");
//            }
//        }
//    }
//}
