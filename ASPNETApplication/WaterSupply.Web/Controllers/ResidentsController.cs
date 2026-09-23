using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ResidentViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Administrator")]
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
        if (model.RegistrationDate > DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(nameof(model.RegistrationDate), "Registration date cannot be in the future.");
        }

        var email = model.Email.Trim();
        if (await context.Residents.AnyAsync(resident => resident.Email.ToLower() == email.ToLower()))
        {
            ModelState.AddModelError(nameof(model.Email), "A resident profile with this email already exists.");
        }
        if (!ModelState.IsValid) return View(model);

        context.Residents.Add(new Resident(0, model.FullName, email, model.Phone, model.Address, model.RegistrationDate, model.IsActive));
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "The resident profile could not be saved. Please check the email and try again.");
            return View(model);
        }
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
        if (model.RegistrationDate > DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(nameof(model.RegistrationDate), "Registration date cannot be in the future.");
        }
        var resident = await context.Residents.FindAsync(id);
        if (resident is null) return NotFound();
        if (await context.Residents.AnyAsync(item => item.ResidentId != id && item.Email.ToLower() == model.Email.Trim().ToLower()))
        {
            ModelState.AddModelError(nameof(model.Email), "A resident profile with this email already exists.");
        }
        if (!ModelState.IsValid) return View(model);
        context.Entry(resident).CurrentValues.SetValues(model);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "The resident profile could not be updated. Please try again.");
            return View(model);
        }
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
