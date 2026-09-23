using System.ComponentModel.DataAnnotations;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models.ConnectionViewModels;

public sealed class ConnectionEditViewModel
{
    [Display(Name = "Resident")]
    [Range(1, int.MaxValue)]
    public int ResidentId { get; set; }

    [Display(Name = "Connection number")]
    [Required, StringLength(40, MinimumLength = 2)]
    [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9-]{1,39}$", ErrorMessage = "Use letters, numbers, and hyphens only.")]
    public string ConnectionNumber { get; set; } = string.Empty;

    [Display(Name = "Connection type")]
    [Required, EnumDataType(typeof(ConnectionType))]
    public ConnectionType ConnectionType { get; set; } = ConnectionType.Residential;

    [Display(Name = "Meter number")]
    [Required, StringLength(40, MinimumLength = 2)]
    [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9-]{1,39}$", ErrorMessage = "Use letters, numbers, and hyphens only.")]
    public string MeterNumber { get; set; } = string.Empty;

    [Display(Name = "Connection date")]
    [Required, DataType(DataType.Date)]
    public DateOnly ConnectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Status")]
    [Required]
    public ConnectionStatus Status { get; set; } = ConnectionStatus.Active;
}
