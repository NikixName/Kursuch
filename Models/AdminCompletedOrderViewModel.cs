namespace Kurs_HTML.Models
{
    public class AdminCompletedOrderViewModel
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; } = "";
        public string ServiceName { get; set; } = "";
        public string PerformerName { get; set; } = "";
        public DateTime DateTime { get; set; }
        public string StatusName { get; set; } = "";
        public DateTime CompletedAt { get; set; } 
        public decimal ServicePrice { get; set; }
    }
}
