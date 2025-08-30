namespace Wg_backend_api.DTO
{
    public class NationBaseInfoDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    }

    public class NationDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int ReligionId { get; set; }
        public int CultureId { get; set; }
        public bool AssignmentIsActive { get; set; }

    }

}
