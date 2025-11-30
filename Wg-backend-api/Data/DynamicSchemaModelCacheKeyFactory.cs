using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Wg_backend_api.Data;

public class DynamicSchemaModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        if (context is GameDbContext gameContext)
        {
            return (context.GetType(), gameContext.Schema, designTime);
        }

        return (context.GetType(), designTime);
    }
}
