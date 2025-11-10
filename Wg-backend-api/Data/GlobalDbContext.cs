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
                optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=postgres;Database=wg");

            }
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameAccess> GameAccesses { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

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
                .HasKey(ga => ga.Id);

            // Konfiguracja relacji między User a GameAccess
            modelBuilder.Entity<GameAccess>()
                .HasOne(ga => ga.User)
                .WithMany(u => u.GameAccesses)
                .HasForeignKey(ga => ga.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracja relacji między Game a GameAccess
            modelBuilder.Entity<GameAccess>()
                .HasOne(ga => ga.Game)
                .WithMany(u => u.GameAccesses)
                .HasForeignKey(ga => ga.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracja relacji jeden-do-wielu między User a Game
            modelBuilder.Entity<Game>()
                .HasOne(ga => ga.Owner)
                .WithMany(u => u.OwnedGames)
                .HasForeignKey(g => g.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithOne(u => u.RefreshToken)
                .HasForeignKey<RefreshToken>(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
