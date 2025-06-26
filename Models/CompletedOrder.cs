using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs_HTML.Models
{
    public class CompletedOrder
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime CompletedAt { get; set; }

        public string PerformerName { get; set; } = "";
        public string ServiceName { get; set; } = "";
    }
}

