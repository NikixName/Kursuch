using System.ComponentModel.DataAnnotations;

namespace Kurs_HTML.Models
{
    public class RegisterViewModel
{
    [Required, StringLength(10)]
    public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(10)]
    public string LastName  { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email     { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password  { get; set; } = string.Empty;
}

}
