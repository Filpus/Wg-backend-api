using Microsoft.AspNetCore.Mvc;
using Wg_backend_api.Data;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    // Wspolna baza dla kontrolerow gry: wyciaga schemat/panstwo z sesji i tworzy
    // GameDbContext wskazujacy na wlasciwy schemat (game_1, game_2, ...).
    // Wczesniej ta sama logika byla powielona niemal identycznie w ~28 kontrolerach
    // pod Controllers/GameControllers - zmiana tutaj naprawia je wszystkie naraz.
    [ApiController]
    public abstract class GameControllerBase : ControllerBase
    {
        protected readonly IGameDbContextFactory GameDbContextFactory;
        protected readonly ISessionDataService SessionDataService;
        protected readonly GameDbContext Context;
        protected readonly int? NationId;

        protected GameControllerBase(IGameDbContextFactory gameDbContextFactory, ISessionDataService sessionDataService)
        {
            this.GameDbContextFactory = gameDbContextFactory;
            this.SessionDataService = sessionDataService;

            string schema = this.SessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }

            this.Context = this.GameDbContextFactory.Create(schema);

            string nationIdStr = this.SessionDataService.GetNation();
            this.NationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);
        }
    }
}
