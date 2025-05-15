namespace Wg_backend_api.DTO
{
    public class ResourceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsMain { get; set; }
        public string Icon { get; set; }
    }

    public class ResourceBalanceDto
    {
        public int ResourceId { get; set; }
        public int CurrentAmount { get; set; }
        public int ArmyMaintenanceExpenses { get; set; }
        public int PopulationExpenses { get; set; }
        public int TradeIncome { get; set; }
        public int TradeExpenses { get; set; }
        public int PopulationProduction { get; set; }
        public int EventBalance { get; set; }
        public int TotalBalance { get; set; }
    }

    public class NationResourceBalanceDto
    {
        public List<ResourceDto> Resources { get; set; } = new();
        public List<ResourceBalanceDto> ResourceBalances { get; set; } = new();
    }

}
