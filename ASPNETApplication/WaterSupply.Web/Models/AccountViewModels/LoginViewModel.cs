using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.AccountViewModels;

public sealed class LoginViewModel
{
    [Display(Name = "Email address")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
