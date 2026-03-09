namespace Kurs_HTML.Models
{
    public class Service
    {
        public int    ServiceId   { get; set; }
        public string Name        { get; set; } = null!;
        public string Category    { get; set; } = null!;
        public decimal BasePrice  { get; set; }
        public string Duration    { get; set; } = null!;
        public string? Notes      { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public string PerformerRole { get; set; } = "Mechanic"; 
    }
}