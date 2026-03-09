using System;
using System.Collections.Generic;

namespace Kurs_HTML.Models
{
    public class TableViewModel
    {

        public List<ServiceViewModel> Services { get; set; } = new();

        public int? ServiceToBook { get; set; }

        public DateTime? PreselectSlot { get; set; }
    }
}
