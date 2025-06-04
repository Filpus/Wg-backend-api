namespace Wg_backend_api.DTO
{
    public class TradeAgreementDTO
    {
        public int? Id { get; set; }    
        public int offeringNationId { get; set; }
        public int receivingNationId { get; set; }
        public bool isActive { get; set; } = true; // Default value is true, indicating the agreement is active
        public int Duration { get; set; }

        public List<ResourceAmountDto> offeredResources { get; set; } = new List<ResourceAmountDto>();
        public List<ResourceAmountDto> requestedResources { get; set; } = new List<ResourceAmountDto>();
    }

  

    public class OfferTradeAgreementDTO
    {
        public int receivingNationId { get; set; }
        public int Duration { get; set; }

        public List<ResourceAmountDto> offeredResources { get; set; } = new List<ResourceAmountDto>();
        public List<ResourceAmountDto> requestedResources { get; set; } = new List<ResourceAmountDto>();
    }
}
