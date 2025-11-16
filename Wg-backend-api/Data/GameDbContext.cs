using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Models;
using Action = Wg_backend_api.Models.Action;

namespace Wg_backend_api.Data
{
    public class GameDbContext : DbContext
    {

        private readonly string _schema;

        public GameDbContext(DbContextOptions<GameDbContext> options, string schema = "") // Fuszera drut this default schema name
            : base(options)
        {
            this._schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Username=postgres;Password=postgres;Database=wg");
            }
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Action> Actions { get; set; }
        public DbSet<UnitType> UnitTypes { get; set; }
        public DbSet<AccessToUnit> AccessToUnits { get; set; }
        public DbSet<UnitOrder> UnitOrders { get; set; }
        public DbSet<ProductionCost> ProductionCosts { get; set; }
        public DbSet<MaintenaceCosts> MaintenaceCosts { get; set; }
        public DbSet<Troop> Troops { get; set; }
        public DbSet<Army> Armies { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Culture> Cultures { get; set; }
        public DbSet<Religion> Religions { get; set; }
        public DbSet<SocialGroup> SocialGroups { get; set; }
        public DbSet<UsedResource> UsedResources { get; set; }
        public DbSet<ProductionShare> ProductionShares { get; set; }
        public DbSet<RelatedEvents> RelatedEvents { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Modifiers> Modifiers { get; set; }
        public DbSet<Faction> Factions { get; set; }
        public DbSet<Localisation> Localisations { get; set; }
        public DbSet<LocalisationResource> LocalisationResources { get; set; }
        public DbSet<Map> Maps { get; set; }
        public DbSet<MapAccess> MapAccesses { get; set; }
        public DbSet<Nation> Nations { get; set; }
        public DbSet<Population> Populations { get; set; }
        public DbSet<TradeAgreement> TradeAgreements { get; set; }
        public DbSet<OfferedResource> OfferedResources { get; set; }
        public DbSet<WantedResource> WantedResources { get; set; }
        public DbSet<PopulationUsedResource> populationUsedResources { get; set; }
        public DbSet<PopulationProductionShare> PopulationProductionShares { get; set; }
        public DbSet<OwnedResources> OwnedResources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            if (!string.IsNullOrEmpty(this._schema))
            {
                modelBuilder.HasDefaultSchema(this._schema);  // Set the schema dynamically based on the provided schema
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.GetTableName());
                entityType.SetSchema(this._schema); // Ustawienie dynamicznego schematu
            }

            modelBuilder.HasDefaultSchema(this._schema);

            modelBuilder.Entity<MapAccess>()
                .HasKey(ma => new { ma.NationId, ma.MapId });

            modelBuilder.Entity<TradeAgreement>()
                   .HasOne(ta => ta.OfferingNation)
                   .WithMany(n => n.OfferedTradeAgreements)
                   .HasForeignKey(ta => ta.OfferingNationId)
                   .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TradeAgreement>()
                .HasOne(ta => ta.ReceivingNation)
                .WithMany(n => n.ReceivedTradeAgreements)
                .HasForeignKey(ta => ta.ReceivingNationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
