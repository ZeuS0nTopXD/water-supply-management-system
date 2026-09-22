using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.AccountViewModels;

public sealed class ResidentProfileViewModel
{
    [Display(Name = "Full name")]
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone number")]
    [Required, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Address")]
    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;
}
