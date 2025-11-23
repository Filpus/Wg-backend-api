namespace Wg_backend_api.DTO
{
    public class LocalisationDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public int Size { get; set; }

        public int Fortification { get; set; }

        public int NationId { get; set; }

    }

    public class LocalisationResourceDTO
    {
        public int? Id { get; set; }
        public int LocationId { get; set; }
        public int ResourceId { get; set; }
        public float Amount { get; set; }
    }

    public class LocalisationResourceInfoDTO
    {
        public int ResourceId { get; set; }
        public string ResourceName { get; set; }
        public float Amount { get; set; }
    }

    public class LocalisationResourceProductionDTO
    {
        public int ResourceId { get; set; }
        public string ResourceName { get; set; }
        public float ProductionAmount { get; set; }
    }

    public class LocalisationGeneralInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int Size { get; set; }

        public int Fortification { get; set; }

        public int PopulationSize { get; set; }
        public float PopulationHappiness { get; set; }
    }

    public class LocalisationDetailsDTO
    {

        public string Name { get; set; }
        public List<LocalisationResourceInfoDTO> Resources { get; set; } = [];
        public List<LocalisationResourceProductionDTO> ResourceProductions { get; set; } = [];
        public List<PopulationGroupDTO> PopulationGroups { get; set; }

    }

}
