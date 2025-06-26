using System;
using System.Collections.Generic;

namespace Kurs_HTML.Models
{
    public class TableViewModel
    {
        /// <summary>
        /// Список всех доступных услуг для каталога
        /// </summary>
        public List<ServiceViewModel> Services { get; set; } = new();

        /// <summary>
        /// Опциональный ID услуги, по которой пользователь сразу кликнул «Записаться»
        /// </summary>
        public int? ServiceToBook { get; set; }

        /// <summary>
        /// Рекомендованный первый свободный слот (если есть) для выбранной услуги
        /// </summary>
        public DateTime? PreselectSlot { get; set; }
    }
}
