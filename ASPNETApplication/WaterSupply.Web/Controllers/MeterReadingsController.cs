using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.MeterReadingViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class MeterReadingsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index() => View(await context.MeterReadings.AsNoTracking().OrderByDescending(reading => reading.ReadingDate).ToListAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.Connections = await context.WaterConnections.AsNoTracking().OrderBy(connection => connection.ConnectionNumber).ToListAsync();
        return View(new MeterReadingEditViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MeterReadingEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Connections = await context.WaterConnections.AsNoTracking().ToListAsync();
            return View(model);
        }

        var reading = new MeterReading(model.WaterConnectionId, model.ReadingDate);
        try
        {
            reading.RecordReading(model.PreviousReading, model.CurrentReading);
            context.MeterReadings.Add(reading);
            await context.SaveChangesAsync();
        }
        catch (Exception exception) when (exception is InvalidOperationException || exception.GetType().Name.Contains("DomainValidation"))
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ViewBag.Connections = await context.WaterConnections.AsNoTracking().ToListAsync();
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}
