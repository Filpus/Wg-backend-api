using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Models;

namespace Wg_backend_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // Dodaj tutaj DbSet dla swoich tabel
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Culture> Cultures { get; set; }
        public DbSet<Religion> Religions { get; set; }
        public DbSet<SocialGroup> SocialGroups { get; set; }
        public DbSet<Nation> Nations { get; set; }
        public DbSet<TradeAgreement> TradeAgreement { get; set; }
        public DbSet<OfferedResource> OfferedResources { get; set; }
        public DbSet<WantedResource> WantedResource { get; set; }
        public DbSet<Assignment> Assignment { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Population> Population { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfiguracja dla SocialGroup
            modelBuilder.Entity<SocialGroup>()
                .Property(s => s.Id)
                .ValueGeneratedOnAdd();

            // Konfiguracja dla Religion
            modelBuilder.Entity<Religion>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Religion>()
                .HasIndex(r => new { r.Id, r.Name })
                .IsUnique();
            // Konfiguracja dla Culture
            modelBuilder.Entity<Culture>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            // Konfiguracja dla Resource
            modelBuilder.Entity<Resource>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Nation>()
                .Property(n => n.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TradeAgreement>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<OfferedResource>()
                .Property(o => o.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<WantedResource>()
                .Property(w => w.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Assignment>()
                .Property(e => e.DateAcquired)
                .HasColumnType("date");
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Localisation> Localisation { get; set; } = default!;

    }
}
