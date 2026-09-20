using WaterSupply.Application.Abstractions;
using WaterSupply.Application.InMemory;
using WaterSupply.Application.Services;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.ConsoleApp.Seed;

public sealed record ConsoleDependencies(ConsoleMenuServices Services, InMemoryRepository<Resident> Residents, InMemoryRepository<WaterConnection> Connections, InMemoryMeterReadingRepository Readings, InMemoryRepository<Bill> Bills, InMemoryRepository<Payment> Payments);

public sealed record ConsoleMenuServices(IResidentService Residents, IWaterConnectionService Connections, IMeterReadingService Readings, IBillingService Billing, IServiceRequestService Requests, IReportService Reports);

public static class ConsoleSeedData
{
    public static ConsoleDependencies Create()
    {
        var residents = new InMemoryRepository<Resident>(resident => resident.Id);
        var connections = new InMemoryRepository<WaterConnection>(connection => connection.Id);
        var readings = new InMemoryMeterReadingRepository();
        var bills = new InMemoryRepository<Bill>(bill => bill.Id);
        var payments = new InMemoryRepository<Payment>(payment => payment.Id);
        var requests = new InMemoryRepository<ServiceRequest>(request => request.Id);
        var residentService = new ResidentService(residents);
        var connectionService = new WaterConnectionService(connections);
        var readingService = new MeterReadingService(readings);
        var billingService = new BillingService(bills, payments, connections);
        var requestService = new ServiceRequestService(requests);
        var reportService = new ReportService(readings, connections, bills, payments);

        var asha = residentService.Create("Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1));
        var ravi = residentService.Create("Ravi Shah", "ravi@example.com", "9876543211", "Lake Road", new DateOnly(2026, 1, 1));
        var ashaConnection = connectionService.Create(asha.Id, "WS-010", ConnectionType.Residential, "M-010", new DateOnly(2026, 1, 5));
        var raviConnection = connectionService.Create(ravi.Id, "WS-020", ConnectionType.Residential, "M-020", new DateOnly(2026, 1, 5));
        readingService.Record(ashaConnection.Id, new DateOnly(2026, 9, 1), 100, 120);
        readingService.Record(raviConnection.Id, new DateOnly(2026, 9, 1), 200, 225);
        billingService.GenerateBill(ashaConnection.Id, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 20, 5, 10, 2, new DateOnly(2026, 10, 15));
        billingService.GenerateBill(raviConnection.Id, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 25, 5, 10, 2, new DateOnly(2026, 10, 15));
        requestService.Create(asha.Id, ashaConnection.Id, ServiceRequestType.Leakage, "Leakage reported near the meter.");

        var services = new ConsoleMenuServices(residentService, connectionService, readingService, billingService, requestService, reportService);
        return new ConsoleDependencies(services, residents, connections, readings, bills, payments);
    }
}
