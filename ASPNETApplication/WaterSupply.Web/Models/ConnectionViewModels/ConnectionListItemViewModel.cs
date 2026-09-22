using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models.ConnectionViewModels;

public sealed class ConnectionListItemViewModel
{
    public int WaterConnectionId { get; init; }
    public string ResidentName { get; init; } = string.Empty;
    public string ConnectionNumber { get; init; } = string.Empty;
    public ConnectionType ConnectionType { get; init; }
    public string MeterNumber { get; init; } = string.Empty;
    public DateOnly ConnectionDate { get; init; }
    public ConnectionStatus Status { get; init; }
}
