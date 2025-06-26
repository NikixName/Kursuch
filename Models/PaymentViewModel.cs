using System.ComponentModel.DataAnnotations;

namespace Kurs_HTML.Models
{
    public class PaymentViewModel
    {
        public int OrderId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Введите имя плательщика")]
        [StringLength(100)]
        public string PayerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите номер карты")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Введите 4 цифры")]
        public string CardPart1 { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{4}$")]
        public string CardPart2 { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{4}$")]
        public string CardPart3 { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{4}$")]
        public string CardPart4 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите срок действия")]
        [RegularExpression(@"^\d{2}/\d{2}$", ErrorMessage = "Формат: ММ/ГГ")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите CVV")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV должен состоять из 3 цифр")]
        public string CVV { get; set; } = string.Empty;
    }
}
