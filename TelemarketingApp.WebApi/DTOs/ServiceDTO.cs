namespace TelemarketingApp.WebApi.DTOs
{
    public class ServiceDTO
    {
        public int ServiceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }
}
