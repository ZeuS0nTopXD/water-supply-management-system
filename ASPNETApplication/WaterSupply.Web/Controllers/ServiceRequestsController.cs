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
public sealed class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServiceRequestsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(ServiceRequestStatus? status)
    {
        var query = _context.ServiceRequests.AsNoTracking();
        if (!User.IsInRole("Administrator"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var residentId = await _context.Residents.Where(resident => resident.IdentityUserId == userId).Select(resident => (int?)resident.Id).SingleOrDefaultAsync();
            query = residentId.HasValue ? query.Where(request => request.ResidentId == residentId.Value) : query.Where(_ => false);
        }
        if (status.HasValue) query = query.Where(request => request.Status == status.Value);
        return View(await query.OrderByDescending(request => request.CreatedAt).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ServiceRequestCreateViewModel();
        if (!User.IsInRole("Administrator"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.ResidentId = await _context.Residents.Where(resident => resident.IdentityUserId == userId).Select(resident => resident.Id).SingleOrDefaultAsync();
        }
        await LoadConnectionsAsync(model.ResidentId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel model)
    {
        if (!User.IsInRole("Administrator"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.ResidentId = await _context.Residents.Where(resident => resident.IdentityUserId == userId).Select(resident => resident.Id).SingleOrDefaultAsync();
        }
        if (model.WaterConnectionId.HasValue && !await _context.WaterConnections.AnyAsync(connection => connection.Id == model.WaterConnectionId.Value && connection.ResidentId == model.ResidentId))
            ModelState.AddModelError(nameof(model.WaterConnectionId), "The selected connection does not belong to this resident.");
        if (!ModelState.IsValid || model.ResidentId <= 0)
        {
            if (model.ResidentId <= 0) ModelState.AddModelError(nameof(model.ResidentId), "A resident account is required.");
            await LoadConnectionsAsync(model.ResidentId);
            return View(model);
        }

        _context.ServiceRequests.Add(ServiceRequest.Create(model.ResidentId, model.WaterConnectionId, model.RequestType, model.Description, DateTime.UtcNow));
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadConnectionsAsync(int residentId)
    {
        var query = _context.WaterConnections.AsNoTracking().OrderBy(connection => connection.ConnectionNumber);
        if (!User.IsInRole("Administrator")) query = query.Where(connection => connection.ResidentId == residentId).OrderBy(connection => connection.ConnectionNumber);
        ViewBag.Connections = await query.ToListAsync();
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet]
    public async Task<IActionResult> UpdateStatus(int? id)
    {
        var request = id is null ? null : await _context.ServiceRequests.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        return request is null ? NotFound() : View(new ServiceRequestStatusViewModel { Status = request.Status, StaffNotes = request.StaffNotes });
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ServiceRequestStatusViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var request = await _context.ServiceRequests.FindAsync(id);
        if (request is null) return NotFound();
        request.UpdateStatus(model.Status, model.StaffNotes);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
