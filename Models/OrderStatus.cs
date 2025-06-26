namespace Kurs_HTML.Models
{
    public class OrderStatus
    {
        public int    OrderStatusId { get; set; } 
        public string Name    { get; set; } = null!; 

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    
    }
}
