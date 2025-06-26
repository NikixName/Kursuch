namespace Kurs_HTML.Models
{
    public class Service
    {
        public int    ServiceId   { get; set; }              // PK
        public string Name        { get; set; } = null!;     // Название услуги
        public string Category    { get; set; } = null!;     // Категория (техобслуживание, мойка и т. д.)
        public decimal BasePrice  { get; set; }              // Базовая стоимость
        public string Duration    { get; set; } = null!;     // Время выполнения (напр. "1-1.5 часа")
        public string? Notes      { get; set; }              // Доп. описание или текст акции

        // Навигационное свойство
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public string PerformerRole { get; set; } = "Mechanic"; 
    }
}