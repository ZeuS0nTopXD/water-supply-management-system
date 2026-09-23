using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ConnectionRequestViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class ConnectionRequestsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(ConnectionRequestStatus? status = null)
    {
        var currentResident = User.IsInRole("Resident")
            ? await FindCurrentResidentAsync()
            : null;
        if (User.IsInRole("Resident") && currentResident is null) return Forbid();

        var query = context.WaterConnectionRequests.AsNoTracking();
        if (currentResident is not null)
        {
            query = query.Where(request => request.ResidentId == currentResident.ResidentId);
        }
        else if (status is not null)
        {
            query = query.Where(request => request.Status == status);
        }

        var requests = await query
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();
        var residentNames = await context.Residents
            .AsNoTracking()
            .Where(resident => requests.Select(request => request.ResidentId).Contains(resident.ResidentId))
            .ToDictionaryAsync(resident => resident.ResidentId, resident => resident.FullName);

        ViewBag.Status = status;
        ViewBag.IsResident = currentResident is not null;
        return View(requests.Select(request => new ConnectionRequestListItemViewModel
        {
            WaterConnectionRequestId = request.WaterConnectionRequestId,
            ResidentId = request.ResidentId,
            ResidentName = residentNames.GetValueOrDefault(request.ResidentId, "Unknown resident"),
            ServiceAddress = request.ServiceAddress,
            RequestedType = request.RequestedType,
            Notes = request.Notes,
            CreatedAt = request.CreatedAt,
            Status = request.Status,
            StaffNotes = request.StaffNotes
        }).ToList());
    }

    [Authorize(Roles = "Resident")]
    public async Task<IActionResult> Create()
    {
        var resident = await FindCurrentResidentAsync();
        if (resident is null) return Forbid();

        return View(new ConnectionRequestCreateViewModel { ServiceAddress = resident.Address });
    }

    [HttpPost, Authorize(Roles = "Resident"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConnectionRequestCreateViewModel model)
    {
        var resident = await FindCurrentResidentAsync();
        if (resident is null) return Forbid();

        if (!ModelState.IsValid) return View(model);

        var hasOpenRequest = await context.WaterConnectionRequests.AnyAsync(request =>
            request.ResidentId == resident.ResidentId
            && (request.Status == ConnectionRequestStatus.Pending || request.Status == ConnectionRequestStatus.InReview));
        if (hasOpenRequest)
        {
            ModelState.AddModelError(string.Empty, "You already have a connection request under review. Wait for the administrator to update it before submitting another.");
            return View(model);
        }

        context.WaterConnectionRequests.Add(new WaterConnectionRequest(
            resident.ResidentId,
            model.RequestedType,
            model.ServiceAddress,
            model.Notes));
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Your connection request could not be submitted. Please try again.");
            return View(model);
        }

        if (TempData is not null)
        {
            TempData["Message"] = "Your connection request was submitted. The administrator will review it and update its status.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateStatus(int id)
    {
        var request = await context.WaterConnectionRequests.FindAsync(id);
        return request is null
            ? NotFound()
            : View(new ConnectionRequestStatusViewModel
            {
                Status = request.Status,
                StaffNotes = request.StaffNotes
            });
    }

    [HttpPost, Authorize(Roles = "Administrator"), ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ConnectionRequestStatusViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var request = await context.WaterConnectionRequests.FindAsync(id);
        if (request is null) return NotFound();

        request.ChangeStatus(model.Status, model.StaffNotes);
        await context.SaveChangesAsync();
        if (TempData is not null)
        {
            TempData["Message"] = "Connection request status updated. If approved, create the resident's connection from the Connections page.";
        }
        return RedirectToAction(nameof(Index));
    }

    private Task<Resident?> FindCurrentResidentAsync()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
        var normalizedEmail = email?.Trim().ToLower();
        return context.Residents.SingleOrDefaultAsync(resident =>
            (identityUserId != null && resident.IdentityUserId == identityUserId)
            || (normalizedEmail != null && resident.Email.ToLower() == normalizedEmail));
    }
}
