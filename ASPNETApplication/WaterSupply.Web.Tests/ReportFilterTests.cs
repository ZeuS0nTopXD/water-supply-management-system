using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Controllers;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ReportViewModels;

namespace WaterSupply.Web.Tests;

public sealed class ReportFilterTests
{
    [Fact]
    public async Task Bill_report_filters_by_status_and_connection_id()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"report-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        db.WaterConnections.AddRange(
            new WaterConnection(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1), 1),
            new WaterConnection(1, "WS-002", ConnectionType.Residential, "M-002", new DateOnly(2026, 1, 1), 2));
        db.MeterReadings.AddRange(
            new MeterReading(1, new DateOnly(2026, 9, 1), 1),
            new MeterReading(2, new DateOnly(2026, 9, 1), 2));
        var first = new Bill(1, 1, new DateOnly(2026, 9, 30), 10, 5m, 1); first.CalculateTotal();
        var second = new Bill(2, 2, new DateOnly(2026, 9, 30), 12, 5m, 2); second.CalculateTotal();
        db.Bills.AddRange(first, second);
        await db.SaveChangesAsync();

        var result = await new ReportsController(db).Index("1", BillStatus.Unpaid);

        var model = result.Should().BeOfType<ViewResult>().Subject.Model.Should().BeOfType<BillReportViewModel>().Subject;
        model.Rows.Should().ContainSingle(row => row.BillId == 1);
    }
}
