using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ReportViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class ReportsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search, BillStatus? status)
    {
        var query = context.Bills
            .AsNoTracking()
            .Join(
                context.WaterConnections.AsNoTracking(),
                bill => bill.WaterConnectionId,
                connection => connection.WaterConnectionId,
                (bill, connection) => new { bill, connection })
            .AsQueryable();
        if (status is not null) query = query.Where(item => item.bill.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item => item.connection.ConnectionNumber.Contains(search));
        }

        var model = new BillReportViewModel
        {
            Search = search,
            Status = status,
            Rows = await query.Select(item => new BillReportRowViewModel
            {
                BillId = item.bill.BillId,
                ConnectionNumber = item.connection.ConnectionNumber,
                BillDate = item.bill.BillDate,
                TotalAmount = item.bill.TotalAmount,
                DueDate = item.bill.DueDate,
                Status = item.bill.Status
            }).ToListAsync()
        };
        return View(model);
    }
}
