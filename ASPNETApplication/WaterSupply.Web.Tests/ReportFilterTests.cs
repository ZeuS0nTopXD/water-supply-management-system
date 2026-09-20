using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Controllers;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ReportViewModels;

namespace WaterSupply.Web.Tests;

public class ReportFilterTests
{
    [Fact]
    public async Task Outstanding_bill_report_applies_status_and_date_filters()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"report-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        db.WaterConnections.Add(WaterConnection.Create(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1), id: 1));
        var overdue = Bill.Create(1, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 10, 5, 10, 0, new DateOnly(2026, 10, 15), 1);
        overdue.MarkOverdue(new DateOnly(2026, 10, 20));
        db.Bills.Add(overdue);
        db.Bills.Add(Bill.Create(2, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31), 10, 5, 10, 0, new DateOnly(2026, 9, 15), 1));
        await db.SaveChangesAsync();
        var controller = new ReportsController(db);

        var result = await controller.OutstandingBills("Overdue", new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30));

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<OutstandingBillsReportViewModel>().Subject;
        model.Rows.Should().ContainSingle(row => row.BillId == 1);
    }
}
