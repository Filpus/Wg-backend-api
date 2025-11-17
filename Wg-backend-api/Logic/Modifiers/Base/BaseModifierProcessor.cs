namespace Wg_backend_api.Logic.Modifiers.Base
{
    //public abstract class BaseModifierProcessor<TEntity> : IModifierProcessor
    //        where TEntity : class
    //{
    //    protected readonly GameDbContext _context;

    //    protected BaseModifierProcessor(GameDbContext context)
    //    {
    //        this._context = context ?? throw new ArgumentNullException(nameof(context));
    //    }

    //    public abstract ModifierType SupportedType { get; }

    //    protected abstract IQueryable<TEntity> GetBaseQuery(int nationId);
    //    protected abstract ConditionBuilder<TEntity> CreateConditionBuilder(IQueryable<TEntity> baseQuery);
    //    protected abstract void ApplyToEntity(TEntity entity, ModifierEffect effect);
    //    protected abstract int GetEntityId(TEntity entity);

    //    protected virtual IQueryable<TEntity> GetTargetEntities(int nationId, Dictionary<string, object> conditions)
    //    {
    //        var baseQuery = GetBaseQuery(nationId);
    //        return CreateConditionBuilder(baseQuery)
    //            .ApplyConditions(conditions)
    //            .Build();
    //    }

    //    public virtual async Task<ModifierApplicationResult> ProcessAsync(int nationId, List<ModifierEffect> effects, GameDbContext context)
    //    {
    //        var result = new ModifierApplicationResult { Success = true };

    //        foreach (var effect in effects)
    //        {
    //            var entities = await GetTargetEntities(nationId, effect.Conditions).ToListAsync();

    //            foreach (var entity in entities)
    //            {
    //                ApplyToEntity(entity, effect);
    //            }

    //            result.AffectedEntities.Add($"affected_count", entities.Count);
    //        }

    //        await context.SaveChangesAsync();
    //        result.Message = $"Zmodyfikowano encje typu {typeof(TEntity).Name}";
    //        return result;
    //    }

    //    public virtual async Task<ModifierApplicationResult> RevertAsync(int nationId, List<ModifierEffect> effects, GameDbContext context)
    //    {
    //        // Implementacja odwracania - na razie pusta
    //        return new ModifierApplicationResult { Success = true, Message = "Revert not implemented yet" };
    //    }

    //}
}
