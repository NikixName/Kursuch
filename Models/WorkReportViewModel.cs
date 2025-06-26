using System.ComponentModel.DataAnnotations;

namespace Kurs_HTML.Models
{
    public class WorkReportViewModel
    {
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Комментарии к выполненной работе")]
        public string Comments { get; set; } = "";
    }
}