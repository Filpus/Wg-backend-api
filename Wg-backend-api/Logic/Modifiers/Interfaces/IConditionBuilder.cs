namespace Wg_backend_api.Logic.Modifiers.Interfaces
{
    public interface IConditionBuilder<TEntity>
    {
        public IConditionBuilder<TEntity> ApplyConditions(Dictionary<string, object> conditions);
        public IQueryable<TEntity> Build();
        public IConditionBuilder<TEntity> Reset();
    }
}
