using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Exceptions;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.MeterReadingViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class MeterReadingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public MeterReadingsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? waterConnectionId, DateOnly? from, DateOnly? to)
    {
        var query = _context.MeterReadings.AsNoTracking();
        if (waterConnectionId.HasValue) query = query.Where(reading => reading.WaterConnectionId == waterConnectionId.Value);
        if (from.HasValue) query = query.Where(reading => reading.ReadingDate >= from.Value);
        if (to.HasValue) query = query.Where(reading => reading.ReadingDate <= to.Value);
        ViewBag.Connections = await _context.WaterConnections.AsNoTracking().OrderBy(connection => connection.ConnectionNumber).ToListAsync();
        return View(await query.OrderByDescending(reading => reading.ReadingDate).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadConnectionsAsync();
        return View(new MeterReadingEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MeterReadingEditViewModel model)
    {
        if (await _context.MeterReadings.AnyAsync(reading => reading.WaterConnectionId == model.WaterConnectionId && reading.ReadingDate == model.ReadingDate))
            ModelState.AddModelError(nameof(model.ReadingDate), "A reading already exists for this connection and date.");
        if (!ModelState.IsValid)
        {
            await LoadConnectionsAsync();
            return View(model);
        }

        try
        {
            _context.MeterReadings.Add(MeterReading.Create(model.WaterConnectionId, model.ReadingDate, model.PreviousReading, model.CurrentReading));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DomainValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadConnectionsAsync();
            return View(model);
        }
    }

    private async Task LoadConnectionsAsync() => ViewBag.Connections = await _context.WaterConnections.AsNoTracking().OrderBy(connection => connection.ConnectionNumber).ToListAsync();
}
