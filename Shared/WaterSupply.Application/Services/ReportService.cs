using WaterSupply.Application.Abstractions;
using WaterSupply.Application.InMemory;
using WaterSupply.Application.Reports;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Application.Services;

public sealed class ReportService : IReportService
{
    private readonly InMemoryWaterSupplyStore _store;

    public ReportService(InMemoryWaterSupplyStore store) => _store = store;

    public SummaryReport GetSummary() => new(
        _store.Residents.Count,
        _store.Connections.Count(connection => connection.Status == ConnectionStatus.Active),
        _store.Readings.Sum(reading => reading.Consumption),
        _store.Bills.Sum(bill => bill.TotalAmount),
        _store.ServiceRequests.Count(request => request.Status != RequestStatus.Closed));
}
