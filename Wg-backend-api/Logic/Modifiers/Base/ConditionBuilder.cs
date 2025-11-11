using System.Text.Json;

namespace Wg_backend_api.Logic.Modifiers.Base
{
    public abstract class ConditionBuilder<TEntity> : IConditionBuilder<TEntity>
           where TEntity : class
    {
        protected IQueryable<TEntity> Query { get; private set; }
        private readonly IQueryable<TEntity> _originalQuery;

        protected ConditionBuilder(IQueryable<TEntity> baseQuery)
        {
            this.Query = baseQuery ?? throw new ArgumentNullException(nameof(baseQuery));
            this._originalQuery = baseQuery;
        }

        public abstract IConditionBuilder<TEntity> ApplyConditions(Dictionary<string, object> conditions);

        public virtual IQueryable<TEntity> Build()
        {
            return this.Query;
        }

        public virtual IConditionBuilder<TEntity> Reset()
        {
            this.Query = this._originalQuery;
            return this;
        }

        protected bool TryGetCondition<T>(Dictionary<string, object> conditions, string key, out T value)
        {
            value = default;

            if (!conditions.TryGetValue(key, out var rawValue))
            {
                return false;
            }

            try
            {
                if (rawValue == null)
                {
                    return false;
                }

                if (rawValue is JsonElement jsonElement)
                {
                    value = JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                }
                else
                {
                    value = (T)Convert.ChangeType(rawValue, typeof(T));
                }

                return value != null && !value.Equals(default(T));
            }
            catch
            {
                return false;
            }
        }
    }
}
