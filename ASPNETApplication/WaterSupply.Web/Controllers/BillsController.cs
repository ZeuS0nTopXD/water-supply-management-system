using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Exceptions;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.BillingViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class BillsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BillsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? status, int? waterConnectionId)
    {
        var query = _context.Bills.AsNoTracking();
        if (!User.IsInRole("Administrator"))
        {
            var residentId = await CurrentResidentIdAsync();
            query = residentId.HasValue
                ? query.Where(bill => _context.WaterConnections.Any(connection => connection.Id == bill.WaterConnectionId && connection.ResidentId == residentId.Value))
                : query.Where(_ => false);
        }
        if (Enum.TryParse<WaterSupply.Domain.Enums.BillStatus>(status, true, out var selectedStatus)) query = query.Where(bill => bill.Status == selectedStatus);
        if (waterConnectionId.HasValue) query = query.Where(bill => bill.WaterConnectionId == waterConnectionId.Value);
        ViewData["Status"] = status;
        return View(await query.OrderByDescending(bill => bill.BillingPeriodEnd).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        var bill = id is null ? null : await _context.Bills.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
        if (bill is null) return NotFound();
        if (!User.IsInRole("Administrator"))
        {
            var residentId = await CurrentResidentIdAsync();
            if (!residentId.HasValue || !await _context.WaterConnections.AnyAsync(connection => connection.Id == bill.WaterConnectionId && connection.ResidentId == residentId.Value)) return Forbid();
        }
        return View(bill);
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadConnectionsAsync();
        return View(new BillCreateViewModel());
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BillCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadConnectionsAsync();
            return View(model);
        }

        try
        {
            _context.Bills.Add(Bill.Create(0, model.BillingPeriodStart, model.BillingPeriodEnd, model.UnitsConsumed, model.RatePerUnit, model.FixedCharge, model.TaxAmount, model.DueDate, model.WaterConnectionId));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DomainValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadConnectionsAsync();
            return View(model);
        }
    }

    private async Task LoadConnectionsAsync() => ViewBag.Connections = await _context.WaterConnections.AsNoTracking().OrderBy(connection => connection.ConnectionNumber).ToListAsync();

    private async Task<int?> CurrentResidentIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _context.Residents.Where(resident => resident.IdentityUserId == userId).Select(resident => (int?)resident.Id).SingleOrDefaultAsync();
    }
}
