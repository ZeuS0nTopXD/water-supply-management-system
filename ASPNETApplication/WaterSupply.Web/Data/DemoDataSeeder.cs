using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, string? primaryIdentityUserId = null)
    {
        if (await context.Residents.AnyAsync()) return;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var month = new DateOnly(today.Year, today.Month, 1);
        var connectionDate = month.AddMonths(-8).AddDays(4);

        var residents = new[]
        {
            new Resident(0, "Asha Patil", "asha@example.com", "9876543210", "Main Road", connectionDate, identityUserId: primaryIdentityUserId),
            new Resident(0, "Ravi Shah", "ravi@example.com", "9876543211", "Lake Road", connectionDate),
            new Resident(0, "Meera Kulkarni", "meera@example.com", "9876543212", "Market Road", connectionDate),
            new Resident(0, "Arjun Deshmukh", "arjun@example.com", "9876543213", "Station Road", connectionDate)
        };
        context.Residents.AddRange(residents);
        await context.SaveChangesAsync();

        var connections = new[]
        {
            new WaterConnection(residents[0].ResidentId, "WS-010", ConnectionType.Residential, "M-010", connectionDate),
            new WaterConnection(residents[1].ResidentId, "WS-020", ConnectionType.Residential, "M-020", connectionDate),
            new WaterConnection(residents[2].ResidentId, "WS-030", ConnectionType.Commercial, "M-030", connectionDate),
            new WaterConnection(residents[3].ResidentId, "WS-040", ConnectionType.Residential, "M-040", connectionDate)
        };
        connections[3].ChangeStatus(ConnectionStatus.Inactive);
        context.WaterConnections.AddRange(connections);
        await context.SaveChangesAsync();

        var readings = new[]
        {
            CreateReading(connections[0].WaterConnectionId, month, 100, 128),
            CreateReading(connections[1].WaterConnectionId, month, 220, 245),
            CreateReading(connections[2].WaterConnectionId, month, 80, 98),
            CreateReading(connections[3].WaterConnectionId, month, 400, 430)
        };
        context.MeterReadings.AddRange(readings);
        await context.SaveChangesAsync();

        var bills = new[]
        {
            CreateBill(connections[0].WaterConnectionId, readings[0].MeterReadingId, month, readings[0].Consumption),
            CreateBill(connections[1].WaterConnectionId, readings[1].MeterReadingId, month, readings[1].Consumption),
            CreateBill(connections[2].WaterConnectionId, readings[2].MeterReadingId, month, readings[2].Consumption),
            CreateBill(connections[3].WaterConnectionId, readings[3].MeterReadingId, month, readings[3].Consumption)
        };
        context.Bills.AddRange(bills);

        var requests = new[]
        {
            new ServiceRequest(residents[0].ResidentId, connections[0].WaterConnectionId, RequestType.Leak, "Leak reported near the meter."),
            new ServiceRequest(residents[1].ResidentId, connections[1].WaterConnectionId, RequestType.NoSupply, "No water supply since yesterday."),
            new ServiceRequest(residents[2].ResidentId, connections[2].WaterConnectionId, RequestType.Other, "Please check the unusual meter reading.")
        };
        requests[1].ChangeStatus(RequestStatus.InProgress, "Technician visit scheduled.");
        requests[2].ChangeStatus(RequestStatus.Closed, "Meter checked and reading confirmed.");
        context.ServiceRequests.AddRange(requests);
        await context.SaveChangesAsync();
    }

    private static MeterReading CreateReading(int connectionId, DateOnly month, decimal previous, decimal current)
    {
        var reading = new MeterReading(connectionId, month);
        reading.RecordReading(previous, current);
        return reading;
    }

    private static Bill CreateBill(int connectionId, int readingId, DateOnly month, decimal consumption)
    {
        var bill = new Bill(connectionId, readingId, month.AddDays(29), (int)consumption, 5m);
        bill.CalculateTotal();
        return bill;
    }
}
