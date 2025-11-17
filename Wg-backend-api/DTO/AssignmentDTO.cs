namespace Wg_backend_api.DTO
{
    public class AssignmentDTO
    {
        public int UserId { get; set; }
        public int NationId { get; set; }
    }

    public class NationsWithAssignmentsDTO
    {
        public class AssignmentInfoDTO
        {
            public int? Id { get; set; }
            public int? UserId { get; set; }
            public string? UserName { get; set; }
        }

        public int? Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string? Flag { get; set; }
        public AssignmentInfoDTO? Assignment { get; set; }
    }
}
