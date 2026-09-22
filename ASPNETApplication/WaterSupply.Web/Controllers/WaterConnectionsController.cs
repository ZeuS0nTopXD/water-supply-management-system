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
        ViewBag.Residents = await context.Residents.AsNoTracking().OrderBy(resident => resident.FullName).ToListAsync();
        return View(new ConnectionEditViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConnectionEditViewModel model)
    {
        if (!ModelState.IsValid) { ViewBag.Residents = await context.Residents.AsNoTracking().ToListAsync(); return View(model); }
        context.WaterConnections.Add(new WaterConnection(model.ResidentId, model.ConnectionNumber, model.ConnectionType, model.MeterNumber, model.ConnectionDate));
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
