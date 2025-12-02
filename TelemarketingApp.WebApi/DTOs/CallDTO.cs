namespace TelemarketingApp.WebApi.DTOs
{
    public class CallDTO
    {
        public int CallId { get; set; }
        public DateTime CallDatetime { get; set; }
        public int Duration { get; set; }
        public string Result { get; set; }
        public string Comment { get; set; }
        public int? EmployeeId { get; set; }
        public int? ClientId { get; set; }
        public ClientDTO Client { get; set; }
        public List<ServiceDTO> Services { get; set; }
    }
}
