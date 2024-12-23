using System.ComponentModel.DataAnnotations;

namespace Blog2.ViewModels.Categories;

public class EditorCategoryViewModel
{
    [Required(ErrorMessage = "o nome é obrigatorio")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "este campo deve contar no minimo 3 caracteres")]
    public string Name { get; set; }


    [Required(ErrorMessage = "o Slug é obrigatorio")]
    public string Slug { get; set; }
}
