using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Services;

namespace WaterSupply.Web.Tests;

public class DashboardQueryTests
{
    [Fact]
    public async Task Dashboard_summary_aggregates_current_month_business_metrics()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"dashboard-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        db.Residents.Add(new Resident(1, "Asha Patil", "asha@example.com", "111", "Main Road", new DateOnly(2026, 1, 1)));
        db.Residents.Add(new Resident(2, "Ravi Shah", "ravi@example.com", "222", "Lake Road", new DateOnly(2026, 1, 1)));
        db.WaterConnections.AddRange(
            WaterConnection.Create(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1), id: 1),
            WaterConnection.Create(2, "WS-002", ConnectionType.Residential, "M-002", new DateOnly(2026, 1, 1), id: 2));
        db.MeterReadings.Add(MeterReading.Create(1, new DateOnly(2026, 9, 1), 100, 120, 1));
        var bill = Bill.Create(1, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 10, 10, 10, 0, new DateOnly(2026, 10, 15), 1);
        bill.RecordPayment(20, PaymentMethod.Cash, new DateTime(2026, 9, 15));
        db.Bills.Add(bill);
        db.Payments.Add(new Payment(1, 1, 20, PaymentMethod.Cash, new DateTime(2026, 9, 15)));
        db.ServiceRequests.Add(ServiceRequest.Create(1, 1, ServiceRequestType.Leakage, "Leak", new DateTime(2026, 9, 10), 1));
        await db.SaveChangesAsync();
        var service = new DashboardQueryService(db);

        var summary = await service.GetSummaryAsync(new DateOnly(2026, 9, 1));

        summary.ResidentCount.Should().Be(2);
        summary.ActiveConnectionCount.Should().Be(2);
        summary.CurrentMonthConsumption.Should().Be(20);
        summary.UnpaidBillAmount.Should().Be(110 - 20);
        summary.CollectionAmount.Should().Be(20);
        summary.OpenServiceRequestCount.Should().Be(1);
    }
}
