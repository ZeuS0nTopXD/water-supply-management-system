using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class WaterConnection
{
    private WaterConnection()
    {
    }

    public WaterConnection(int residentId, string connectionNumber, ConnectionType type, string meterNumber, DateOnly connectionDate, int id = 0)
    {
        if (residentId <= 0) throw new DomainValidationException("A valid resident is required.");
        ResidentId = residentId;
        ConnectionNumber = Required(connectionNumber, nameof(connectionNumber));
        ConnectionType = type;
        MeterNumber = Required(meterNumber, nameof(meterNumber));
        ConnectionDate = connectionDate;
        Status = ConnectionStatus.Active;
        WaterConnectionId = id;
    }

    public int WaterConnectionId { get; private set; }
    public int ResidentId { get; private set; }
    public string ConnectionNumber { get; private set; } = string.Empty;
    public ConnectionType ConnectionType { get; private set; }
    public string MeterNumber { get; private set; } = string.Empty;
    public DateOnly ConnectionDate { get; private set; }
    public ConnectionStatus Status { get; private set; }

    public void UpdateDetails(int residentId, string connectionNumber, ConnectionType type, string meterNumber, DateOnly connectionDate)
    {
        if (residentId <= 0) throw new DomainValidationException("A valid resident is required.");
        ResidentId = residentId;
        ConnectionNumber = Required(connectionNumber, nameof(connectionNumber));
        ConnectionType = type;
        MeterNumber = Required(meterNumber, nameof(meterNumber));
        ConnectionDate = connectionDate;
    }

    public void ChangeStatus(ConnectionStatus status) => Status = status;

    private static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainValidationException($"{name} is required.");
        return value.Trim();
    }
}
