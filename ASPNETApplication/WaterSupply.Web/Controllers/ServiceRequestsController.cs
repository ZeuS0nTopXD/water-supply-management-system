using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ServiceRequestViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class ServiceRequestsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var isResident = ControllerContext.HttpContext?.User?.IsInRole("Resident") == true;
        var currentResident = isResident
            ? await FindCurrentResidentAsync()
            : null;
        if (isResident && currentResident is null) return Forbid();

        var residents = await context.Residents
            .AsNoTracking()
            .ToDictionaryAsync(resident => resident.ResidentId, resident => resident.FullName);
        var connections = await context.WaterConnections
            .AsNoTracking()
            .ToDictionaryAsync(connection => connection.WaterConnectionId, connection => connection.ConnectionNumber);
        var requestQuery = context.ServiceRequests.AsNoTracking();
        if (currentResident is not null)
        {
            requestQuery = requestQuery.Where(request => request.ResidentId == currentResident.ResidentId);
        }

        var requests = await requestQuery
            .AsNoTracking()
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();
        var model = requests.Select(request => new ServiceRequestListItemViewModel
        {
            ServiceRequestId = request.ServiceRequestId,
            ResidentId = request.ResidentId,
            ResidentName = residents.GetValueOrDefault(request.ResidentId, $"Resident #{request.ResidentId}"),
            ConnectionNumber = request.WaterConnectionId is int connectionId
                ? connections.GetValueOrDefault(connectionId)
                : null,
            RequestType = request.RequestType,
            Description = request.Description,
            CreatedAt = request.CreatedAt,
            Status = request.Status
        });

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var isResident = ControllerContext.HttpContext?.User?.IsInRole("Resident") == true;
        var currentResident = isResident
            ? await FindCurrentResidentAsync()
            : null;
        if (isResident && currentResident is null) return Forbid();

        await LoadCreateOptionsAsync(currentResident);
        return View(new ServiceRequestCreateViewModel { ResidentId = currentResident?.ResidentId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel model)
    {
        var isResident = ControllerContext.HttpContext?.User?.IsInRole("Resident") == true;
        var currentResident = isResident
            ? await FindCurrentResidentAsync()
            : null;
        if (isResident && currentResident is null) return Forbid();

        if (currentResident is not null)
        {
            var postedResidentId = model.ResidentId;
            ModelState.Remove(nameof(model.ResidentId));
            model.ResidentId = currentResident.ResidentId;
            if (postedResidentId != 0 && postedResidentId != currentResident.ResidentId)
            {
                ModelState.AddModelError(nameof(model.ResidentId), "You can only create requests for your own account.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadCreateOptionsAsync(currentResident);
            return View(model);
        }

        if (!await context.Residents.AnyAsync(resident => resident.ResidentId == model.ResidentId))
        {
            ModelState.AddModelError(nameof(model.ResidentId), "Select a valid resident.");
        }

        if (model.WaterConnectionId is not null)
        {
            var connection = await context.WaterConnections
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.WaterConnectionId == model.WaterConnectionId);
            if (connection is null)
            {
                ModelState.AddModelError(nameof(model.WaterConnectionId), "Select a valid water connection.");
            }
            else if (connection.ResidentId != model.ResidentId)
            {
                ModelState.AddModelError(nameof(model.WaterConnectionId), "The connection must belong to the selected resident.");
            }
            else if (connection.Status != ConnectionStatus.Active)
            {
                ModelState.AddModelError(nameof(model.WaterConnectionId), "The selected water connection is inactive.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadCreateOptionsAsync(currentResident);
            return View(model);
        }

        context.ServiceRequests.Add(new ServiceRequest(model.ResidentId, model.WaterConnectionId, model.RequestType, model.Description));
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "The request could not be saved. Check that the selected records are still available.");
            await LoadCreateOptionsAsync(currentResident);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateStatus(int id)
    {
        var request = await context.ServiceRequests.FindAsync(id);
        return request is null ? NotFound() : View(new ServiceRequestStatusViewModel { Status = request.Status, StaffNotes = request.StaffNotes });
    }

    [HttpPost, Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ServiceRequestStatusViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var request = await context.ServiceRequests.FindAsync(id);
        if (request is null) return NotFound();
        request.ChangeStatus(model.Status, model.StaffNotes);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCreateOptionsAsync(Resident? currentResident)
    {
        ViewBag.CurrentResidentName = currentResident?.FullName;
        ViewBag.Residents = currentResident is null
            ? await context.Residents
                .AsNoTracking()
                .OrderBy(resident => resident.FullName)
                .ToListAsync()
            : [];
        var connections = context.WaterConnections
            .AsNoTracking()
            .Where(connection => connection.Status == ConnectionStatus.Active);
        if (currentResident is not null)
        {
            connections = connections.Where(connection => connection.ResidentId == currentResident.ResidentId);
        }

        ViewBag.Connections = await connections
            .OrderBy(connection => connection.ConnectionNumber)
            .ToListAsync();
    }

    private Task<Resident?> FindCurrentResidentAsync()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
        return context.Residents.SingleOrDefaultAsync(resident =>
            (identityUserId != null && resident.IdentityUserId == identityUserId)
            || (email != null && resident.Email == email));
    }
}
