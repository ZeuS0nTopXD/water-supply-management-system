using WaterSupply.Application.Abstractions;
using WaterSupply.Application.InMemory;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Application.Services;

public sealed class WaterConnectionService : IWaterConnectionService
{
    private readonly InMemoryWaterSupplyStore _store;

    public WaterConnectionService(InMemoryWaterSupplyStore store) => _store = store;

    public WaterConnection Create(int residentId, string connectionNumber, ConnectionType type, string meterNumber, DateOnly connectionDate)
    {
        if (_store.Connections.Any(connection => connection.ConnectionNumber.Equals(connectionNumber.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainValidationException("Connection number must be unique.");
        }

        var connection = new WaterConnection(residentId, connectionNumber, type, meterNumber, connectionDate, _store.Connections.Count + 1);
        _store.Connections.Add(connection);
        return connection;
    }

    public IReadOnlyList<WaterConnection> Search(string? query)
    {
        var normalized = query?.Trim() ?? string.Empty;
        if (normalized.Length == 0) return _store.Connections;
        return _store.Connections.Where(connection => connection.ConnectionNumber.Contains(normalized, StringComparison.OrdinalIgnoreCase)
            || connection.MeterNumber.Contains(normalized, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
