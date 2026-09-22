using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.AccountViewModels;

public sealed class RegisterViewModel
{
    [Display(Name = "Full name")]
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email address")]
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone number")]
    [Required, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Address")]
    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Confirm password")]
    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
