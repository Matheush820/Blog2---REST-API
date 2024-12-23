using System.ComponentModel.DataAnnotations;

namespace Blog2.ViewModels.Accounts;

public class UploadImageViewModel
{
    [Required(ErrorMessage ="Imagem invalida")]
    public string Base65Image { get; set; }
}
