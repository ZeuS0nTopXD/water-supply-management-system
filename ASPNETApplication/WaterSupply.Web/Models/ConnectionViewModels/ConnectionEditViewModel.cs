using System.ComponentModel.DataAnnotations;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models.ConnectionViewModels;

public sealed class ConnectionEditViewModel
{
    [Range(1, int.MaxValue)]
    public int ResidentId { get; set; }

    [Required, StringLength(40)]
    public string ConnectionNumber { get; set; } = string.Empty;

    [Required]
    public ConnectionType ConnectionType { get; set; } = ConnectionType.Residential;

    [Required, StringLength(40)]
    public string MeterNumber { get; set; } = string.Empty;

    [Required]
    public DateOnly ConnectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    public ConnectionStatus Status { get; set; } = ConnectionStatus.Active;
}
