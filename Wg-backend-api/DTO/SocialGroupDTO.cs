using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Wg_backend_api.DTO
{
    public class SocialGroupDTO
    {
        public int? Id { get; set; }

 
        public string Name { get; set; }

        public float BaseHappiness { get; set; } // Typ float do przechowywania wartości zmiennoprzecinkowych

        public int Volunteers { get; set; } // Typ int do przechowywania liczby
    }


    public class SocialGroupInfoDTO
    {
        public string Name { get; set; }
        public float BaseHappiness { get; set; } // Typ float do przechowywania wartości zmiennoprzecinkowych
        public int Volunteers { get; set; } // Typ int do przechowywania liczby
        public List<ResourceAmountDto> ConsumedResources { get; set; }
        public List<ResourceAmountDto> ProducedResources { get; set; }
    }
}
