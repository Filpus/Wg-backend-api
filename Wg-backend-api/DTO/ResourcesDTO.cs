namespace Wg_backend_api.DTO
{
    public class ResourceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsMain { get; set; }
        public string? Icon { get; set; }
    }

    public class CreateResourceDto
    {
        public string Name { get; set; }
        public bool IsMain { get; set; }
        public IFormFile? IconFile { get; set; }
    }

    public class ResourceBalanceDto
    {
        public int ResourceId { get; set; }
        public float CurrentAmount { get; set; }
        public float ArmyMaintenanceExpenses { get; set; }
        public float PopulationExpenses { get; set; }
        public float TradeIncome { get; set; }
        public float TradeExpenses { get; set; }
        public float PopulationProduction { get; set; }
        public float EventBalance { get; set; }
        public float TotalBalance { get; set; }
    }

    public class NationResourceBalanceDto
    {
        public List<ResourceDto> Resources { get; set; } = new();
        public List<ResourceBalanceDto> ResourceBalances { get; set; } = new();
    }

    public class ResourceAmountDto
    {

        public int ResourceId { get; set; }
        public string? ResourceName { get; set; }
        public float Amount { get; set; }
    }

}
