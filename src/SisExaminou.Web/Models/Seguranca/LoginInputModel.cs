using System.ComponentModel.DataAnnotations;

namespace SisExaminou.Web.Models.Seguranca;

public sealed class LoginInputModel
{
    [Required(ErrorMessage = "Informe o login.")]
    [StringLength(100, ErrorMessage = "O login deve possuir até 100 caracteres.")]
    [Display(Name = "Login")]
    public string? Login { get; set; }

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(128, ErrorMessage = "A senha deve possuir até 128 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string? Senha { get; set; }

    public string? ReturnUrl { get; set; }
}
