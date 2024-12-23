using System.ComponentModel.DataAnnotations;

namespace Blog2.ViewModels.Accounts;

public class RegisterViewModel
{
    [Required(ErrorMessage = "O nome é Obrigatório")]
    public string Name { get; set; }


    [Required(ErrorMessage = "O Email é Obrigatório")]
    [EmailAddress(ErrorMessage = "Emai é invalido")]
    public string Email { get; set; }

}
