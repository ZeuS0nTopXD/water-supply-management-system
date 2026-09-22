using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ConnectionViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class WaterConnectionsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = context.WaterConnections.AsNoTracking().OrderBy(connection => connection.ConnectionNumber).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(connection => connection.ConnectionNumber.Contains(search) || connection.MeterNumber.Contains(search));
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await LoadResidentsAsync();
        return View(new ConnectionEditViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConnectionEditViewModel model)
    {
        if (!ModelState.IsValid) { await LoadResidentsAsync(); return View(model); }

        if (!await context.Residents.AnyAsync(resident => resident.ResidentId == model.ResidentId))
        {
            ModelState.AddModelError(nameof(model.ResidentId), "Select a valid resident.");
        }

        if (await context.WaterConnections.AnyAsync(connection => connection.ConnectionNumber == model.ConnectionNumber))
        {
            ModelState.AddModelError(nameof(model.ConnectionNumber), "That connection number is already in use.");
        }

        if (!ModelState.IsValid) { await LoadResidentsAsync(); return View(model); }

        context.WaterConnections.Add(new WaterConnection(model.ResidentId, model.ConnectionNumber, model.ConnectionType, model.MeterNumber, model.ConnectionDate));
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "The connection could not be saved. Check that the resident and connection number are still available.");
            await LoadResidentsAsync();
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadResidentsAsync()
    {
        ViewBag.Residents = await context.Residents
            .AsNoTracking()
            .OrderBy(resident => resident.FullName)
            .ToListAsync();
    }
}
