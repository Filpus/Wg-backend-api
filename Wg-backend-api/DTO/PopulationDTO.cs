using System.ComponentModel.DataAnnotations.Schema;

namespace Wg_backend_api.DTO
{
    public class PopulationDTO
    {

        public int? Id { get; set; }

        public int ReligionId { get; set; }

        public int CultureId { get; set; }

        public int SocialGroupId { get; set; }

        public int LocationId { get; set; }
        public float Happiness { get; set; }
    }

    public class PopulationReligiousGroupDTO
    {
        public string Religion { get; set; }
        public int Amount { get; set; }
        public float Happiness { get; set; }
    }

    public class PopulationCultureGroupDTO
    {
        public string Culture { get; set; }
        public int Amount { get; set; }
        public float Happiness { get; set; }
    }

    public class PopulationSocialGroupDTO
    {
        public string SocialGroup { get; set; }
        public int Amount { get; set; }
        public float Happiness { get; set; }
    }

    public class PopulationGroupDTO
    {
        public string Religion { get; set; }
        public string Culture { get; set; }
        public string SocialGroup { get; set; }
        public int Amount { get; set; }
        public float Happiness { get; set; }
    }
    public class TotalPopulationInfoDTO
    {
        public int TotalPopulation { get; set; }
        public float AverageHappiness { get; set; }

    }
}