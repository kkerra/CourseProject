namespace TelemarketingApp.WebApi.DTOs
{
    public class CreateCallDto
    {
        public string Address { get; set; } = null!;
        public string ServiceTitle { get; set; } = null!;
        public string? EquipmentName { get; set; }
        public string ClientFullName { get; set; } = null!;
        public string ClientPhone { get; set; } = null!;
        public string? ClientEmail { get; set; }
        public DateTime ConnectionDate { get; set; }
        public string? Comment { get; set; }
        public int Duration { get; set; }
        public string Result { get; set; } = null!;
        public int? EmployeeId { get; set; }
    }
}
