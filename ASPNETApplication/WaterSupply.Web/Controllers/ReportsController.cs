using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ReportViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class ReportsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search, BillStatus? status)
    {
        var query = context.Bills.AsNoTracking().OrderByDescending(bill => bill.BillDate).AsQueryable();
        if (status is not null) query = query.Where(bill => bill.Status == status);
        if (int.TryParse(search, out var connectionId)) query = query.Where(bill => bill.WaterConnectionId == connectionId);

        var model = new BillReportViewModel
        {
            Search = search,
            Status = status,
            Rows = await query.Select(bill => new BillReportRowViewModel
            {
                BillId = bill.BillId,
                WaterConnectionId = bill.WaterConnectionId,
                BillDate = bill.BillDate,
                TotalAmount = bill.TotalAmount,
                DueDate = bill.DueDate,
                Status = bill.Status
            }).ToListAsync()
        };
        return View(model);
    }
}
