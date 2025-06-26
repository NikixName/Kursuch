using System;
using System.ComponentModel.DataAnnotations;

namespace Kurs_HTML.Models
{
    public class Receipt
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PayerName { get; set; }

        public virtual Order Order { get; set; } = null!;

    }
}
