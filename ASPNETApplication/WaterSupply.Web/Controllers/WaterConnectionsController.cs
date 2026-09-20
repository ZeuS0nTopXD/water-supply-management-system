using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ConnectionViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class WaterConnectionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public WaterConnectionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.WaterConnections.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(connection => connection.ConnectionNumber.Contains(search) || connection.MeterNumber.Contains(search));
        ViewData["Search"] = search;
        return View(await query.OrderBy(connection => connection.ConnectionNumber).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        var connection = id is null ? null : await _context.WaterConnections.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        return connection is null ? NotFound() : View(connection);
    }

    [HttpGet]
    public IActionResult Create() => View(new ConnectionEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConnectionEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _context.WaterConnections.AnyAsync(connection => connection.ConnectionNumber == model.ConnectionNumber)) ModelState.AddModelError(nameof(model.ConnectionNumber), "Connection number must be unique.");
        if (!ModelState.IsValid) return View(model);
        _context.WaterConnections.Add(WaterConnection.Create(model.ResidentId, model.ConnectionNumber, model.ConnectionType, model.MeterNumber, model.ConnectionDate, model.Status));
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        var connection = id is null ? null : await _context.WaterConnections.FindAsync(id);
        return connection is null ? NotFound() : View(ToViewModel(connection));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ConnectionEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var connection = await _context.WaterConnections.FindAsync(id);
        if (connection is null) return NotFound();
        connection.ChangeStatus(model.Status);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        var connection = id is null ? null : await _context.WaterConnections.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        return connection is null ? NotFound() : View(connection);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var connection = await _context.WaterConnections.FindAsync(id);
        if (connection is null) return NotFound();
        _context.WaterConnections.Remove(connection);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private static ConnectionEditViewModel ToViewModel(WaterConnection connection) => new()
    {
        ResidentId = connection.ResidentId,
        ConnectionNumber = connection.ConnectionNumber,
        ConnectionType = connection.ConnectionType,
        MeterNumber = connection.MeterNumber,
        ConnectionDate = connection.ConnectionDate,
        Status = connection.Status
    };
}
