using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.MeterReadingViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class MeterReadingsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = await context.MeterReadings
            .AsNoTracking()
            .Join(
                context.WaterConnections.AsNoTracking(),
                reading => reading.WaterConnectionId,
                connection => connection.WaterConnectionId,
                (reading, connection) => new MeterReadingListItemViewModel
                {
                    ConnectionNumber = connection.ConnectionNumber,
                    ReadingDate = reading.ReadingDate,
                    PreviousReading = reading.PreviousReading,
                    CurrentReading = reading.CurrentReading,
                    Consumption = reading.Consumption
                })
            .OrderByDescending(reading => reading.ReadingDate)
            .ThenBy(reading => reading.ConnectionNumber)
            .ToListAsync();

        return View(model);
    }

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

        if (model.ReadingDate > DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(nameof(model.ReadingDate), "Reading date cannot be in the future.");
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
            .Where(connection => connection.Status == WaterSupply.Domain.Enums.ConnectionStatus.Active)
            .OrderBy(connection => connection.ConnectionNumber)
            .ToListAsync();
    }
}
