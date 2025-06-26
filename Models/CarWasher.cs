// Models/CarWasher.cs
using System.ComponentModel.DataAnnotations;

namespace Kurs_HTML.Models
{
    public class CarWasher
    {
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string LastName  { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email     { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), StringLength(128)]
        public string Password  { get; set; } = string.Empty;

        // Новые поля:
        [StringLength(20)]
        public string? Phone    { get; set; }

        [StringLength(50)]
        public string? Car      { get; set; }

        [StringLength(20)]
        public string? License  { get; set; }

        [StringLength(256)]
        public string? AvatarPath { get; set; }
    }
}
