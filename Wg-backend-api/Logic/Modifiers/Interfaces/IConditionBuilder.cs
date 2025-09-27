namespace Wg_backend_api.Logic.Modifiers.Interfaces
{
    public interface IConditionBuilder<TEntity>
    {
        IConditionBuilder<TEntity> ApplyConditions(Dictionary<string, object> conditions);
        IQueryable<TEntity> Build();
        IConditionBuilder<TEntity> Reset();
    }
}
