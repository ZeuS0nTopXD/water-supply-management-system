using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Services;

namespace WaterSupply.Web.Tests;

public sealed class DashboardQueryTests
{
    [Fact]
    public async Task Dashboard_summary_aggregates_business_metrics()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"dashboard-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        db.Residents.AddRange(
            new Resident(1, "Asha Patil", "asha@example.com", "111", "Main Road", new DateOnly(2026, 1, 1)),
            new Resident(2, "Ravi Shah", "ravi@example.com", "222", "Lake Road", new DateOnly(2026, 1, 1)));
        db.WaterConnections.AddRange(
            new WaterConnection(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1), 1),
            new WaterConnection(2, "WS-002", ConnectionType.Residential, "M-002", new DateOnly(2026, 1, 1), 2));
        var reading = new MeterReading(1, new DateOnly(2026, 9, 1), 1);
        reading.RecordReading(100, 120);
        db.MeterReadings.Add(reading);
        var bill = new Bill(1, 1, new DateOnly(2026, 9, 30), 20, 5m, 1);
        bill.CalculateTotal();
        db.Bills.Add(bill);
        db.ServiceRequests.Add(new ServiceRequest(1, 1, RequestType.Leak, "Leak"));
        await db.SaveChangesAsync();

        var summary = await new DashboardQueryService(db).GetSummaryAsync(new DateOnly(2026, 9, 1));

        summary.ResidentCount.Should().Be(2);
        summary.ActiveConnectionCount.Should().Be(2);
        summary.CurrentMonthConsumption.Should().Be(20);
        summary.UnpaidBillAmount.Should().Be(100);
        summary.OpenServiceRequestCount.Should().Be(1);
    }
}
