using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kurs_HTML.Models
{
    public class BookViewModel
    {
        public int ServiceId { get; set; }

        public string? ServiceName { get; set; }

        public List<DateTime> BusySlots { get; set; } = new();

        [Display(Name = "Дата и время")]
        public DateTime SelectedDateTime { get; set; }
    }

    public class AssignedPersonItem
    {
        public string Value { get; set; } = null!;
        public string Text { get; set; } = null!;
    }
}
