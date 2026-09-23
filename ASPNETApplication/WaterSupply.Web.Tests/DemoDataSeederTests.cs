using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Web.Data;
using WaterSupply.Web.Services;

namespace WaterSupply.Web.Tests;

public sealed class DemoDataSeederTests
{
    [Fact]
    public async Task Demo_seed_populates_presentation_ready_dashboard_metrics()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"demo-{Guid.NewGuid():N}")
            .Options;
        await using var db = new ApplicationDbContext(options);

        await DemoDataSeeder.SeedAsync(db);

        var currentMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        var summary = await new DashboardQueryService(db).GetSummaryAsync(currentMonth);

        summary.ResidentCount.Should().Be(4);
        summary.ActiveConnectionCount.Should().Be(3);
        summary.CurrentMonthConsumption.Should().Be(101);
        summary.UnpaidBillAmount.Should().Be(505);
        summary.OpenServiceRequestCount.Should().Be(2);
        summary.PendingConnectionRequestCount.Should().Be(1);
        (await db.Residents.CountAsync()).Should().Be(4);
        (await db.WaterConnections.CountAsync()).Should().Be(4);
        (await db.MeterReadings.CountAsync()).Should().Be(4);
        (await db.Bills.CountAsync()).Should().Be(4);
        (await db.ServiceRequests.CountAsync()).Should().Be(3);
        (await db.WaterConnectionRequests.CountAsync()).Should().Be(1);
    }
}
