using WaterSupply.Application.Abstractions;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Application.Services;

public sealed class WaterConnectionService : IWaterConnectionService
{
    private readonly IRepository<WaterConnection> _connections;

    public WaterConnectionService(IRepository<WaterConnection> connections)
    {
        _connections = connections;
    }

    public WaterConnection Create(int residentId, string connectionNumber, ConnectionType connectionType, string meterNumber, DateOnly connectionDate)
    {
        if (_connections.GetAll().Any(connection => connection.ConnectionNumber.Equals(connectionNumber.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainValidationException("Connection number must be unique.");
        }

        var connection = WaterConnection.Create(residentId, connectionNumber, connectionType, meterNumber, connectionDate);
        _connections.Add(connection);
        return connection;
    }

    public IReadOnlyList<WaterConnection> Search(string query)
    {
        var normalized = query?.Trim() ?? string.Empty;
        if (normalized.Length == 0) return _connections.GetAll();
        return _connections.GetAll().Where(connection => connection.ConnectionNumber.Contains(normalized, StringComparison.OrdinalIgnoreCase)
            || connection.MeterNumber.Contains(normalized, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
