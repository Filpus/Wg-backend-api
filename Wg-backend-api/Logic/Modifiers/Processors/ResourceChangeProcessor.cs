using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers.Processors
{
    public class ResourceChangeProcessor : IModifierProcessor
    {
        private readonly GameDbContext _context;
        private readonly ILogger<ResourceChangeProcessor> _logger;

        public ResourceChangeProcessor(GameDbContext context, ILogger<ResourceChangeProcessor> logger)
        {
            _context = context;
            _logger = logger;
        }

        public ModifierType SupportedType => ModifierType.ResourceChange;

        public async Task<ModifierApplicationResult> ProcessAsync(int nationId, List<ModifierEffect> effects, GameDbContext context)
        {
            return new ModifierApplicationResult
            {
                Success = true,
                Message = "ResourceChange obliczane dynamicznie"
            };
        }

        public Task<ModifierApplicationResult> RevertAsync(int nationId, List<ModifierEffect> effects, GameDbContext context)
        {
            return Task.FromResult(new ModifierApplicationResult
            {
                Success = true,
                Message = "ResourceChange nie wymaga cofania"
            });
        }


        public async Task<float> CalculateChangeAsync(int nationId, ResourceBalanceDto balance)
        {
            var modifiers = await _context.RelatedEvents
                .Where(re => re.NationId == nationId)
                .SelectMany(re => re.Event.Modifiers)
                .Where(m => m.modiferType.ToString() == ModifierType.ResourceChange.ToString())
                .ToListAsync();


            if (!modifiers.Any())
                return 0f;

            float totalChange = 0f;
            int resourceId = balance.ResourceId;

            foreach (var mod in modifiers)
            {
                var rawEffects = JsonSerializer.Deserialize<List<ModifierEffect>>(mod.Effects);
                if (rawEffects == null) continue;

                foreach (var raw in rawEffects)
                {
                    // Zamiast słownika: typowane warunki
                    var conditions = JsonSerializer.Deserialize<ResourceConditions>(
                        JsonSerializer.Serialize(raw.Conditions)
                    );
                    if (conditions == null || conditions.ResourceId != resourceId)
                        continue;

                    var operation = Enum.Parse<ModifierOperation>(raw.Operation, true);
                    float current = await _context.LocalisationResources
                        .Include(lr => lr.Location)
                        .Where(lr => lr.Location.NationId == nationId
                                     && lr.ResourceId == resourceId)
                        .SumAsync(lr => lr.Amount);


                    if (operation == ModifierOperation.Add)
                    {
                        totalChange += raw.Value;
                    }
                    else
                    {
                        float updated = OperationProcessor.ApplyOperation(current, raw.Value, operation);
                        totalChange += updated - current;
                    }
                }
            }

            return totalChange;
        }

    }
}
