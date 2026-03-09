namespace Kurs_HTML.Models
{
public class OrderViewModel
{
    public int OrderId { get; set; }
    public string ServiceName { get; set; } = null!;
    public DateTime DateCreated { get; set; }
    public DateTime DateTime { get; set; }
    public string Name { get; set; } = null!;
    public string PerformerName { get; set; } = null!;

    public bool IsPaid { get; set; }

}
}
