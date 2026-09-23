using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.AccountViewModels;

public sealed class ResidentProfileViewModel
{
    [Display(Name = "Full name")]
    [Required, StringLength(120, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone number")]
    [Required, StringLength(30)]
    [RegularExpression(@"^\+?[0-9\s().-]{7,20}$", ErrorMessage = "Enter a valid phone number.")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Address")]
    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;
}
