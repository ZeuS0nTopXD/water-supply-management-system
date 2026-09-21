using WaterSupply.Domain.Entities;

namespace WaterSupply.Application.InMemory;

public sealed class InMemoryWaterSupplyStore
{
    public List<Resident> Residents { get; init; } = new();
    public List<WaterConnection> Connections { get; init; } = new();
    public List<MeterReading> Readings { get; init; } = new();
    public List<Bill> Bills { get; init; } = new();
    public List<ServiceRequest> ServiceRequests { get; init; } = new();
}
