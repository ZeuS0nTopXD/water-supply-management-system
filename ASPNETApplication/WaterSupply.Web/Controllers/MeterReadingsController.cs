using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Exceptions;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.MeterReadingViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class MeterReadingsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index() => View(await context.MeterReadings.AsNoTracking().OrderByDescending(reading => reading.ReadingDate).ToListAsync());

    public async Task<IActionResult> Create()
    {
        await LoadConnectionsAsync();
        return View(new MeterReadingEditViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MeterReadingEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadConnectionsAsync();
            return View(model);
        }

        if (!await context.WaterConnections.AnyAsync(connection => connection.WaterConnectionId == model.WaterConnectionId))
        {
            ModelState.AddModelError(nameof(model.WaterConnectionId), "Select a valid water connection.");
        }

        if (await context.MeterReadings.AnyAsync(reading =>
                reading.WaterConnectionId == model.WaterConnectionId &&
                reading.ReadingDate == model.ReadingDate))
        {
            ModelState.AddModelError(nameof(model.ReadingDate), "A reading already exists for this connection and date.");
        }

        if (!ModelState.IsValid)
        {
            await LoadConnectionsAsync();
            return View(model);
        }

        var reading = new MeterReading(model.WaterConnectionId, model.ReadingDate);
        try
        {
            reading.RecordReading(model.PreviousReading, model.CurrentReading);
            context.MeterReadings.Add(reading);
            await context.SaveChangesAsync();
        }
        catch (DomainValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadConnectionsAsync();
            return View(model);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "The reading could not be saved. Check that the selected connection is still available.");
            await LoadConnectionsAsync();
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadConnectionsAsync()
    {
        ViewBag.Connections = await context.WaterConnections
            .AsNoTracking()
            .OrderBy(connection => connection.ConnectionNumber)
            .ToListAsync();
    }
}
