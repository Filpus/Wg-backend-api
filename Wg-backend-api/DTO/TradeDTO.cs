using Wg_backend_api.Enums;

namespace Wg_backend_api.DTO
{
    public class TradeAgreementDTO
    {
        public int? Id { get; set; }
        public int offeringNationId { get; set; }
        public int receivingNationId { get; set; }
        public TradeStatus Status { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; } = "";

        public List<ResourceAmountDto> offeredResources { get; set; } = new List<ResourceAmountDto>();
        public List<ResourceAmountDto> requestedResources { get; set; } = new List<ResourceAmountDto>();
    }


    public class TradeAgreementInfoDTO
    {
        public int? Id { get; set; }
        public string OfferingNationName { get; set; }
        public string ReceivingNationName { get; set; }
        public string Status { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; } = "";
        public List<ResourceAmountDto> OfferedResources { get; set; } = new List<ResourceAmountDto>();
        public List<ResourceAmountDto> RequestedResources { get; set; } = new List<ResourceAmountDto>();
    }

    public class OfferTradeAgreementDTO
    {
        public int receivingNationId { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; } = "";
        public List<ResourceAmountDto> offeredResources { get; set; } = new List<ResourceAmountDto>();
        public List<ResourceAmountDto> requestedResources { get; set; } = new List<ResourceAmountDto>();
    }

}
