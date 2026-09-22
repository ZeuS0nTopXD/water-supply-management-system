using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.AccountViewModels;

public sealed class ChangePasswordViewModel
{
    [Display(Name = "Current password")]
    [Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Display(Name = "New password")]
    [Required, DataType(DataType.Password), MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Display(Name = "Confirm new password")]
    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
