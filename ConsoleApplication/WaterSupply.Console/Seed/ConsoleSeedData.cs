using WaterSupply.Application.Abstractions;
using WaterSupply.Application.InMemory;
using WaterSupply.Application.Services;
using WaterSupply.Domain.Enums;

namespace WaterSupply.ConsoleApp.Seed;

public sealed record ConsoleDependencies(ConsoleMenuServices Services);

public sealed record ConsoleMenuServices(
    IResidentService Residents,
    IWaterConnectionService Connections,
    IMeterReadingService Readings,
    IBillingService Billing,
    IServiceRequestService Requests,
    IReportService Reports);

public static class ConsoleSeedData
{
    public static ConsoleDependencies Create()
    {
        var store = new InMemoryWaterSupplyStore();
        var residents = new ResidentService(store);
        var connections = new WaterConnectionService(store);
        var readings = new MeterReadingService(store);
        var billing = new BillingService(store);
        var requests = new ServiceRequestService(store);
        var reports = new ReportService(store);

        var asha = residents.Create("Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1));
        var ravi = residents.Create("Ravi Shah", "ravi@example.com", "9876543211", "Lake Road", new DateOnly(2026, 1, 1));
        var ashaConnection = connections.Create(asha.ResidentId, "WS-010", ConnectionType.Residential, "M-010", new DateOnly(2026, 1, 5));
        var raviConnection = connections.Create(ravi.ResidentId, "WS-020", ConnectionType.Residential, "M-020", new DateOnly(2026, 1, 5));
        var ashaReading = readings.Record(ashaConnection.WaterConnectionId, new DateOnly(2026, 9, 1), 100, 120);
        var raviReading = readings.Record(raviConnection.WaterConnectionId, new DateOnly(2026, 9, 1), 200, 225);
        billing.Generate(ashaConnection.WaterConnectionId, ashaReading.MeterReadingId, new DateOnly(2026, 9, 1), (int)ashaReading.Consumption, 5m);
        billing.Generate(raviConnection.WaterConnectionId, raviReading.MeterReadingId, new DateOnly(2026, 9, 1), (int)raviReading.Consumption, 5m);
        requests.Create(asha.ResidentId, ashaConnection.WaterConnectionId, RequestType.Leak, "Leak reported near the meter.");

        return new ConsoleDependencies(new ConsoleMenuServices(residents, connections, readings, billing, requests, reports));
    }
}
