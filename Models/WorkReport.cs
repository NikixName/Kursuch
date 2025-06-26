using System;
using System.ComponentModel.DataAnnotations;

namespace Kurs_HTML.Models
{
    public class WorkReport
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Comments { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public virtual Order Order { get; set; } = null!;
        
    }
}
