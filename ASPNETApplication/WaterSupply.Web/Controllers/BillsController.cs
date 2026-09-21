using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.BillingViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class BillsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(BillStatus? status = null)
    {
        var query = context.Bills.AsNoTracking().OrderByDescending(bill => bill.BillDate).AsQueryable();
        if (status is not null) query = query.Where(bill => bill.Status == status);
        ViewBag.Status = status;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Connections = await context.WaterConnections.AsNoTracking().ToListAsync();
        ViewBag.Readings = await context.MeterReadings.AsNoTracking().ToListAsync();
        return View(new BillCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BillCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Connections = await context.WaterConnections.AsNoTracking().ToListAsync();
            ViewBag.Readings = await context.MeterReadings.AsNoTracking().ToListAsync();
            return View(model);
        }

        var bill = new Bill(model.WaterConnectionId, model.MeterReadingId, model.BillDate, model.UnitsConsumed, model.RatePerUnit);
        bill.CalculateTotal();
        context.Bills.Add(bill);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
