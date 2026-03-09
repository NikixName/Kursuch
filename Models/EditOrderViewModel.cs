using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kurs_HTML.Models
{
    public class EditOrderViewModel
    {
        public int OrderId { get; set; }

        [Display(Name = "Услуга")]
        public string ServiceName { get; set; } = null!;

        [Display(Name = "Запланировано")]
        [DataType(DataType.DateTime)]
        public DateTime CurrentDateTime { get; set; }

        public List<OrderStatus> AllStatuses { get; set; } = new();
        public int SelectedStatusId { get; set; }

        public List<AssignedPersonItem> AllPerformers { get; set; } = new();
        [Display(Name = "Исполнитель")]
        public string? PerformerRoleAndId { get; set; }
    }
}
