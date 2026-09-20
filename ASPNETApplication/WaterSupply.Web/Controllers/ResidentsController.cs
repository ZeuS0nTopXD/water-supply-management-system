using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ResidentViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class ResidentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ResidentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Residents.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(resident => resident.FullName.Contains(search) || resident.Email.Contains(search) || resident.Phone.Contains(search));
        ViewData["Search"] = search;
        return View(await query.OrderBy(resident => resident.FullName).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var resident = await _context.Residents.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        return resident is null ? NotFound() : View(resident);
    }

    [HttpGet]
    public IActionResult Create() => View(new ResidentEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResidentEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        _context.Residents.Add(new Resident(0, model.FullName, model.Email, model.Phone, model.Address, model.RegistrationDate, isActive: model.IsActive));
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        var resident = id is null ? null : await _context.Residents.FindAsync(id);
        return resident is null ? NotFound() : View(ToViewModel(resident));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResidentEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var resident = await _context.Residents.FindAsync(id);
        if (resident is null) return NotFound();
        resident.UpdateContact(model.FullName, model.Email, model.Phone, model.Address);
        if (model.IsActive) resident.Activate(); else resident.Deactivate();
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        var resident = id is null ? null : await _context.Residents.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        return resident is null ? NotFound() : View(resident);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var resident = await _context.Residents.FindAsync(id);
        if (resident is null) return NotFound();
        _context.Residents.Remove(resident);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private static ResidentEditViewModel ToViewModel(Resident resident) => new()
    {
        FullName = resident.FullName,
        Email = resident.Email,
        Phone = resident.Phone,
        Address = resident.Address,
        RegistrationDate = resident.RegistrationDate,
        IsActive = resident.IsActive
    };
}
