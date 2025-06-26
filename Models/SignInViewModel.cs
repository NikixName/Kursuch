using System.ComponentModel.DataAnnotations;

namespace Kurs_HTML.Models
{
public class SignInViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

}