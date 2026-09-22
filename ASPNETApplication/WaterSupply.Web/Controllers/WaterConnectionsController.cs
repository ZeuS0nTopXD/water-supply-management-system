using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ConnectionViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class WaterConnectionsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = context.WaterConnections
            .AsNoTracking()
            .Join(
                context.Residents.AsNoTracking(),
                connection => connection.ResidentId,
                resident => resident.ResidentId,
                (connection, resident) => new ConnectionListItemViewModel
                {
                    WaterConnectionId = connection.WaterConnectionId,
                    ResidentName = resident.FullName,
                    ConnectionNumber = connection.ConnectionNumber,
                    ConnectionType = connection.ConnectionType,
                    MeterNumber = connection.MeterNumber,
                    ConnectionDate = connection.ConnectionDate,
                    Status = connection.Status
                })
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(connection => connection.ConnectionNumber.Contains(search) || connection.MeterNumber.Contains(search) || connection.ResidentName.Contains(search));
        ViewBag.Search = search;
        return View(await query.OrderBy(connection => connection.ConnectionNumber).ToListAsync());
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

    public async Task<IActionResult> Edit(int id)
    {
        var connection = await context.WaterConnections.FindAsync(id);
        if (connection is null) return NotFound();

        await LoadResidentsAsync();
        return View(new ConnectionEditViewModel
        {
            ResidentId = connection.ResidentId,
            ConnectionNumber = connection.ConnectionNumber,
            ConnectionType = connection.ConnectionType,
            MeterNumber = connection.MeterNumber,
            ConnectionDate = connection.ConnectionDate,
            Status = connection.Status
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ConnectionEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadResidentsAsync();
            return View(model);
        }

        var connection = await context.WaterConnections.FindAsync(id);
        if (connection is null) return NotFound();

        if (!await context.Residents.AnyAsync(resident => resident.ResidentId == model.ResidentId))
        {
            ModelState.AddModelError(nameof(model.ResidentId), "Select a valid resident.");
        }

        if (await context.WaterConnections.AnyAsync(item =>
                item.WaterConnectionId != id && item.ConnectionNumber == model.ConnectionNumber))
        {
            ModelState.AddModelError(nameof(model.ConnectionNumber), "That connection number is already in use.");
        }

        if (!ModelState.IsValid)
        {
            await LoadResidentsAsync();
            return View(model);
        }

        connection.UpdateDetails(model.ResidentId, model.ConnectionNumber, model.ConnectionType, model.MeterNumber, model.ConnectionDate);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "The connection could not be updated. Check that the resident and connection number are still available.");
            await LoadResidentsAsync();
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var connection = await context.WaterConnections.FindAsync(id);
        if (connection is null) return NotFound();

        var hasHistory = await context.MeterReadings.AnyAsync(reading => reading.WaterConnectionId == id)
            || await context.Bills.AnyAsync(bill => bill.WaterConnectionId == id)
            || await context.ServiceRequests.AnyAsync(request => request.WaterConnectionId == id);

        if (hasHistory)
        {
            connection.ChangeStatus(ConnectionStatus.Inactive);
            TempData["Message"] = $"Connection {connection.ConnectionNumber} was deactivated because it has billing or service history.";
        }
        else
        {
            context.WaterConnections.Remove(connection);
            TempData["Message"] = $"Connection {connection.ConnectionNumber} was removed.";
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Message"] = "The connection could not be removed. It may have related records.";
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
