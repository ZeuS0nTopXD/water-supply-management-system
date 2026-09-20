using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class WaterConnection : Entity
{
    private WaterConnection()
    {
    }

    private WaterConnection(int id, int residentId, string connectionNumber, ConnectionType connectionType, string meterNumber, DateOnly connectionDate, ConnectionStatus status)
        : base(id)
    {
        if (residentId <= 0) throw new DomainValidationException("A valid resident is required.");
        ResidentId = residentId;
        ConnectionNumber = Required(connectionNumber, nameof(connectionNumber));
        ConnectionType = connectionType;
        MeterNumber = Required(meterNumber, nameof(meterNumber));
        ConnectionDate = connectionDate;
        Status = status;
    }

    public int ResidentId { get; private set; }
    public string ConnectionNumber { get; private set; } = string.Empty;
    public ConnectionType ConnectionType { get; private set; }
    public string MeterNumber { get; private set; } = string.Empty;
    public DateOnly ConnectionDate { get; private set; }
    public ConnectionStatus Status { get; private set; }

    public static WaterConnection Create(int residentId, string connectionNumber, ConnectionType connectionType, string meterNumber, DateOnly connectionDate, ConnectionStatus status = ConnectionStatus.Active, int id = 0)
        => new(id, residentId, connectionNumber, connectionType, meterNumber, connectionDate, status);

    public void ChangeStatus(ConnectionStatus status) => Status = status;

    private static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainValidationException($"{name} is required.");
        return value.Trim();
    }
}
