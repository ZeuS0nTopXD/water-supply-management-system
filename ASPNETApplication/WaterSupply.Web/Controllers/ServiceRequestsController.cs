using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ServiceRequestViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class ServiceRequestsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var residents = await context.Residents
            .AsNoTracking()
            .ToDictionaryAsync(resident => resident.ResidentId, resident => resident.FullName);
        var requests = await context.ServiceRequests
            .AsNoTracking()
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();
        var model = requests.Select(request => new ServiceRequestListItemViewModel
        {
            ServiceRequestId = request.ServiceRequestId,
            ResidentId = request.ResidentId,
            ResidentName = residents.GetValueOrDefault(request.ResidentId, $"Resident #{request.ResidentId}"),
            RequestType = request.RequestType,
            Description = request.Description,
            CreatedAt = request.CreatedAt,
            Status = request.Status
        });

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Residents = await context.Residents.AsNoTracking().ToListAsync();
        ViewBag.Connections = await context.WaterConnections.AsNoTracking().ToListAsync();
        return View(new ServiceRequestCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Residents = await context.Residents.AsNoTracking().ToListAsync();
            ViewBag.Connections = await context.WaterConnections.AsNoTracking().ToListAsync();
            return View(model);
        }

        context.ServiceRequests.Add(new ServiceRequest(model.ResidentId, model.WaterConnectionId, model.RequestType, model.Description));
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> UpdateStatus(int id)
    {
        var request = await context.ServiceRequests.FindAsync(id);
        return request is null ? NotFound() : View(new ServiceRequestStatusViewModel { Status = request.Status, StaffNotes = request.StaffNotes });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ServiceRequestStatusViewModel model)
    {
        var request = await context.ServiceRequests.FindAsync(id);
        if (request is null) return NotFound();
        request.ChangeStatus(model.Status, model.StaffNotes);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
