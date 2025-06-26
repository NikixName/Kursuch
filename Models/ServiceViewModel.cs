
namespace Kurs_HTML.Models
{
    public class ServiceViewModel
    {
        public int     ServiceId     { get; set; }
        public string  Name          { get; set; } = null!;
        public string? Notes         { get; set; }
        public string  Category      { get; set; } = null!;
        public decimal BasePrice     { get; set; }
        public string  Duration      { get; set; } = null!;


        public string PerformerRole  { get; set; } = "Mechanic";

        public bool    Available     { get; set; }
        public string? DiscountText  { get; set; }
    }
}

