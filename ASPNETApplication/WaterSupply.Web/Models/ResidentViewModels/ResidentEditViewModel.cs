using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.ResidentViewModels;

public sealed class ResidentEditViewModel
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

    [Display(Name = "Registration date")]
    [Required]
    public DateOnly RegistrationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Active resident")]
    public bool IsActive { get; set; } = true;
}
