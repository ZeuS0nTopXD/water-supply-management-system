using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.BillingViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class BillsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(BillStatus? status = null)
    {
        var query = context.Bills
            .AsNoTracking()
            .Join(
                context.WaterConnections.AsNoTracking(),
                bill => bill.WaterConnectionId,
                connection => connection.WaterConnectionId,
                (bill, connection) => new BillListItemViewModel
                {
                    BillId = bill.BillId,
                    ConnectionNumber = connection.ConnectionNumber,
                    BillDate = bill.BillDate,
                    TotalAmount = bill.TotalAmount,
                    DueDate = bill.DueDate,
                    Status = bill.Status
                })
            .AsQueryable();
        if (status is not null) query = query.Where(bill => bill.Status == status);
        ViewBag.Status = status;
        return View(await query.OrderByDescending(bill => bill.BillDate).ThenBy(bill => bill.ConnectionNumber).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await LoadCreateOptionsAsync();
        return View(new BillCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BillCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadCreateOptionsAsync();
            return View(model);
        }

        var connection = await context.WaterConnections
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.WaterConnectionId == model.WaterConnectionId);
        if (connection is null)
        {
            ModelState.AddModelError(nameof(model.WaterConnectionId), "Select a valid water connection.");
        }
        else if (connection.Status != ConnectionStatus.Active)
        {
            ModelState.AddModelError(nameof(model.WaterConnectionId), "The selected water connection is inactive.");
        }

        var reading = await context.MeterReadings
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.MeterReadingId == model.MeterReadingId);
        if (reading is null)
        {
            ModelState.AddModelError(nameof(model.MeterReadingId), "Select a valid meter reading.");
        }
        else if (reading.WaterConnectionId != model.WaterConnectionId)
        {
            ModelState.AddModelError(nameof(model.MeterReadingId), "The meter reading must belong to the selected connection.");
        }
        else if (model.BillDate < reading.ReadingDate)
        {
            ModelState.AddModelError(nameof(model.BillDate), "Bill date cannot be earlier than the meter reading date.");
        }

        if (await context.Bills.AnyAsync(bill =>
                bill.WaterConnectionId == model.WaterConnectionId &&
                bill.BillDate == model.BillDate))
        {
            ModelState.AddModelError(nameof(model.BillDate), "A bill already exists for this connection and date.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCreateOptionsAsync();
            return View(model);
        }

        var bill = new Bill(model.WaterConnectionId, model.MeterReadingId, model.BillDate, model.UnitsConsumed, model.RatePerUnit);
        bill.CalculateTotal();
        context.Bills.Add(bill);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "The bill could not be saved. Check that the selected records are still available.");
            await LoadCreateOptionsAsync();
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCreateOptionsAsync()
    {
        ViewBag.Connections = await context.WaterConnections
            .AsNoTracking()
            .Where(connection => connection.Status == WaterSupply.Domain.Enums.ConnectionStatus.Active)
            .OrderBy(connection => connection.ConnectionNumber)
            .ToListAsync();
        ViewBag.Readings = await context.MeterReadings
            .AsNoTracking()
            .OrderByDescending(reading => reading.ReadingDate)
            .ToListAsync();
    }
}
