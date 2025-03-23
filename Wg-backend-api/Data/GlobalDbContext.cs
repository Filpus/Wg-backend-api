using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Models;

namespace Wg_backend_api.Data
{
    public class GlobalDbContext : DbContext
    {
        public GlobalDbContext(DbContextOptions<GlobalDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=Filip1234;Database=wg");

            }
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameAccess> GameAccesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Global");

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Name)
                .IsUnique();

            modelBuilder.Entity<Game>()
                .HasKey(g => g.Id);

            modelBuilder.Entity<Game>()
                .HasIndex(g => g.Name)
                .IsUnique();

            modelBuilder.Entity<GameAccess>()
                .HasKey(ga => new { ga.UserId, ga.GameId });

            // Konfiguracja relacji między User a GameAccess
            modelBuilder.Entity<GameAccess>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ga => ga.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracja relacji między Game a GameAccess
            modelBuilder.Entity<GameAccess>()
                .HasOne<Game>()
                .WithMany()
                .HasForeignKey(ga => ga.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracja relacji jeden-do-wielu między User a Game
            modelBuilder.Entity<Game>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(g => g.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
