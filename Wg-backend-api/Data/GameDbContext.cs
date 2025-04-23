using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Data;
using Wg_backend_api.Models;
using Action = Wg_backend_api.Models.Action;

namespace Wg_backend_api.Data
{
    public class GameDbContext : DbContext
    {

        private readonly string _schema;

        public GameDbContext(DbContextOptions<GameDbContext> options, string schema = "" ) // Fuszera drut this default schema name
            : base(options)
        {
            
            _schema = schema  ?? throw new ArgumentNullException(nameof(schema));
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            if (!string.IsNullOrEmpty(_schema))
            {
                modelBuilder.HasDefaultSchema(_schema);  // Set the schema dynamically based on the provided schema
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.GetTableName());
                entityType.SetSchema(_schema); // Ustawienie dynamicznego schematu

            }
            modelBuilder.HasDefaultSchema(_schema);
            modelBuilder.Entity<AccessToUnit>()
                .HasKey(atu => atu.Id);
            modelBuilder.Entity<AccessToUnit>()
                .HasOne<UnitType>()
                .WithMany()
                .HasForeignKey(atu => atu.UnitTypeId);
            modelBuilder.Entity<AccessToUnit>()
                .HasOne<Player>()
                .WithMany()
                .HasForeignKey(atu => atu.UserId);

            modelBuilder.Entity<UnitOrder>()
                .HasKey(uo => uo.Id);
            modelBuilder.Entity<UnitOrder>()
                .HasOne<UnitType>()
                .WithMany()
                .HasForeignKey(uo => uo.UnitTypeId);
            modelBuilder.Entity<UnitOrder>()
                .HasOne<Nation>()
                .WithMany()
                .HasForeignKey(uo => uo.NationId);

            modelBuilder.Entity<ProductionCost>()
                .HasKey(pc => pc.Id);
            modelBuilder.Entity<ProductionCost>()
                .HasOne<UnitType>()
                .WithMany()
                .HasForeignKey(pc => pc.UnitTypeId);
            modelBuilder.Entity<ProductionCost>()
                .HasOne<Resource>()
                .WithMany()
                .HasForeignKey(pc => pc.ResourceId);

            modelBuilder.Entity<MaintenaceCosts>()
                .HasKey(mc => mc.Id);
            modelBuilder.Entity<MaintenaceCosts>()
                .HasOne<UnitType>()
                .WithMany()
                .HasForeignKey(mc => mc.UnitTypeId);
            modelBuilder.Entity<MaintenaceCosts>()
                .HasOne<Resource>()
                .WithMany()
                .HasForeignKey(mc => mc.ResourceId);

            modelBuilder.Entity<Troop>()
                .HasKey(t => t.Id);
            modelBuilder.Entity<Troop>()
                .HasOne<UnitType>()
                .WithMany()
                .HasForeignKey(t => t.UnitTypeId);
            modelBuilder.Entity<Troop>()
                .HasOne<Army>()
                .WithMany()
                .HasForeignKey(t => t.Army);

            modelBuilder.Entity<Army>()
                .HasKey(a => a.Id);
            modelBuilder.Entity<Army>()
                .HasOne<Nation>()
                .WithMany()
                .HasForeignKey(a => a.NationId);
            modelBuilder.Entity<Army>()
                .HasOne<Localisation>()
                .WithMany()
                .HasForeignKey(a => a.LocationId);

            modelBuilder.Entity<Action>()
                .HasKey(a => a.Id);
            modelBuilder.Entity<Action>()
                .HasOne<Nation>()
                .WithMany()
                .HasForeignKey(a => a.NationId);

            modelBuilder.Entity<Assignment>()
                .HasKey(a => a.Id);
            modelBuilder.Entity<Assignment>()
                .HasOne<Nation>()
                .WithMany()
                .HasForeignKey(a => a.NationId);
            modelBuilder.Entity<Assignment>()
                .HasOne<Player>()
                .WithMany()
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<RelatedEvents>()
                .HasKey(re => re.Id);
            modelBuilder.Entity<RelatedEvents>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(re => re.EventId);
            modelBuilder.Entity<RelatedEvents>()
                .HasOne<Nation>()
                .WithMany()
                .HasForeignKey(re => re.NationId);

            modelBuilder.Entity<Modifiers>()
                .HasKey(m => m.Id);
            modelBuilder.Entity<Modifiers>()
                .HasOne<Event>()
                .WithMany()
                .HasForeignKey(m => m.EventId);
            modelBuilder.Entity<Modifiers>()
                .HasOne<Resource>()
                .WithMany()
                .HasForeignKey(m => m.ResourceId);
            modelBuilder.Entity<Modifiers>()
                .HasOne<SocialGroup>()
                .WithMany()
                .HasForeignKey(m => m.SocialGroupId);
            modelBuilder.Entity<Modifiers>()
                .HasOne<Culture>()
                .WithMany()
                .HasForeignKey(m => m.CultureId);
            modelBuilder.Entity<Modifiers>()
                .HasOne<Religion>()
                .WithMany()
                .HasForeignKey(m => m.ReligionId);

            modelBuilder.Entity<Faction>()
                .HasKey(f => f.Id);
            modelBuilder.Entity<Faction>()
                .HasOne<Nation>()
                .WithMany()
                .HasForeignKey(f => f.NationId);

            modelBuilder.Entity<Localisation>()
                .HasKey(l => l.Id);
            modelBuilder.Entity<Localisation>()
                .HasOne<Nation>()
                .WithMany()
                .HasForeignKey(l => l.NationId);

            modelBuilder.Entity<LocalisationResource>()
                .HasKey(lr => lr.Id);
            modelBuilder.Entity<LocalisationResource>()
                .HasOne<Localisation>()
                .WithMany()
                .HasForeignKey(lr => lr.LocationId);    

            modelBuilder.Entity<MapAccess>()
                .HasKey(ma => new { ma.UserId, ma.MapId });
            modelBuilder.Entity<MapAccess>()
                .HasOne<Player>()
                .WithMany()
                .HasForeignKey(ma => ma.UserId);
            modelBuilder.Entity<MapAccess>()
                .HasOne<Map>()
                .WithMany()
                .HasForeignKey(ma => ma.MapId);

            modelBuilder.Entity<Population>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<Population>()
                .HasOne<Religion>()
                .WithMany()
                .HasForeignKey(p => p.ReligionId);
            modelBuilder.Entity<Population>()
                .HasOne<Culture>()
                .WithMany()
                .HasForeignKey(p => p.CultureId);
            modelBuilder.Entity<Population>()
                .HasOne<SocialGroup>()
                .WithMany()
                .HasForeignKey(p => p.SocialGroupId);
            modelBuilder.Entity<Population>()
                .HasOne<Localisation>()
                .WithMany()
                .HasForeignKey(p => p.LocationId);

            modelBuilder.Entity<TradeAgreement>()
                .HasKey(ta => ta.Id);
            modelBuilder.Entity<TradeAgreement>()
                .HasOne<Nation>()
                .WithMany()
                .HasForeignKey(ta => ta.OferingNationId);
            modelBuilder.Entity<TradeAgreement>()
                .HasOne<Nation>()
                .WithMany()
                .HasForeignKey(ta => ta.ReceivingNationId);

            modelBuilder.Entity<OfferedResource>()
                .HasKey(or => or.Id);
            modelBuilder.Entity<OfferedResource>()
                .HasOne<Resource>()
                .WithMany()
                .HasForeignKey(or => or.ResourceId);
            modelBuilder.Entity<OfferedResource>()
                .HasOne<TradeAgreement>()
                .WithMany()
                .HasForeignKey(or => or.TradeAgreementId);

            modelBuilder.Entity<WantedResource>()
                .HasKey(wr => wr.Id);
            modelBuilder.Entity<WantedResource>()
                .HasOne<Resource>()
                .WithMany()
                .HasForeignKey(wr => wr.ResourceId);
            modelBuilder.Entity<WantedResource>()
                .HasOne<TradeAgreement>()
                .WithMany()
                .HasForeignKey(wr => wr.TradeAgreementId);

            //modelBuilder.HasDefaultSchema(_schema);
        }
    }
}
