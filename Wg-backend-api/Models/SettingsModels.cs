using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;

namespace Wg_backend_api.Models
{
    public interface Settings<T>
    {
        static abstract Task<T> GetRowAsync(int id, GameDbContext context);
        static abstract Task<T> GetRowAsync(GameDbContext context);

    }

    [Table("armySettings")]
    public class ArmySettings : Settings<ArmySettings>
    {
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [Column("nameOfSettingsSet")]
        public string? NameOfSettingsSet { get; set; }

        [Column("useMeleeAtack")]
        public bool UseMeleeAtack { get; set; }

        [Column("useRangeAtack")]
        public bool UseRangeAtack { get; set; }

        [Column("useDefense")]
        public bool UseDefense { get; set; }

        [Column("useSpeed")]
        public bool UseSpeed { get; set; }

        [Column("useMorale")]
        public bool UseMorale { get; set; }

        [Column("useMaintanace")]
        public bool UseMaintanace { get; set; }


        public static async Task<ArmySettings> GetRowAsync(int id, GameDbContext context)
        {

            ArmySettings? entity = null;

            if (id > 0)
            {
                entity = await context.Set<ArmySettings>().FindAsync(id);
                if (entity != null) return entity;
            }

            entity = await context.Set<ArmySettings>().FirstOrDefaultAsync();
            if (entity != null) return entity;

            var newEntity = new ArmySettings
            {
                NameOfSettingsSet = "Default",
                UseMeleeAtack = true,
                UseRangeAtack = true,
                UseDefense = true,
                UseSpeed = true,
                UseMorale = true,
                UseMaintanace = true,
            };

            context.Set<ArmySettings>().Add(newEntity);
            await context.SaveChangesAsync();

            return newEntity;
        }
        public static async Task<ArmySettings> GetRowAsync(GameDbContext context)
        {

            ArmySettings? entity = null;
            entity = await context.Set<ArmySettings>().FirstOrDefaultAsync();
            if (entity != null)
                return entity;

            var newEntity = new ArmySettings
            {
                NameOfSettingsSet = "Default",
                UseMeleeAtack = true,
                UseRangeAtack = true,
                UseDefense = true,
                UseSpeed = true,
                UseMorale = true,
                UseMaintanace = true,
            };

            context.Set<ArmySettings>().Add(newEntity);
            await context.SaveChangesAsync();

            return newEntity;
        }
    }
}
