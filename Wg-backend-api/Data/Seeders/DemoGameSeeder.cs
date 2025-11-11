namespace Wg_backend_api.Data.Seeders
{
    public class DemoGameSeeder
    {
        private readonly GameDbContext _context;

        public DemoGameSeeder(GameDbContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}
