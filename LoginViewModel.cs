using System.ComponentModel.DataAnnotations;

namespace Blog2.ViewModels.Accounts;

public class LoginViewModel
{
    [Required(ErrorMessage = "informe o email")]
    [EmailAddress(ErrorMessage = "Email invalido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Informe a senha")]
    public string Password { get; set; }
}
