using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ResidentViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class ResidentsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search, string? error)
    {
        var query = context.Residents.AsNoTracking().OrderBy(resident => resident.FullName).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(resident => resident.FullName.Contains(search) || resident.Email.Contains(search));
        ViewBag.Search = search;
        ViewBag.Error = error;
        return View(await query.ToListAsync());
    }

    public IActionResult Create() => View(new ResidentEditViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResidentEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        context.Residents.Add(new Resident(0, model.FullName, model.Email, model.Phone, model.Address, model.RegistrationDate, model.IsActive));
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var resident = await context.Residents.FindAsync(id);
        return resident is null ? NotFound() : View(new ResidentEditViewModel
        {
            FullName = resident.FullName,
            Email = resident.Email,
            Phone = resident.Phone,
            Address = resident.Address,
            RegistrationDate = resident.RegistrationDate,
            IsActive = resident.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResidentEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var resident = await context.Residents.FindAsync(id);
        if (resident is null) return NotFound();
        context.Entry(resident).CurrentValues.SetValues(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var resident = await context.Residents.FindAsync(id);
        if (resident is null) return NotFound();

        var hasConnections = await context.WaterConnections.AnyAsync(connection => connection.ResidentId == id);
        var hasRequests = await context.ServiceRequests.AnyAsync(request => request.ResidentId == id);
        if (hasConnections || hasRequests)
        {
            return RedirectToAction(nameof(Index), new
            {
                error = "Remove related connections and requests before deleting this resident."
            });
        }

        context.Residents.Remove(resident);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
