using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Resident")]
public sealed class ResidentPortalController : Controller
{
    private readonly ApplicationDbContext _context;

    public ResidentPortalController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var resident = await CurrentResidentAsync();
        if (resident is null) return Forbid();
        var bills = await _context.Bills.AsNoTracking()
            .Join(_context.WaterConnections, bill => bill.WaterConnectionId, connection => connection.Id, (bill, connection) => new { bill, connection })
            .Where(item => item.connection.ResidentId == resident.Id)
            .Select(item => item.bill)
            .OrderByDescending(bill => bill.BillingPeriodEnd)
            .ToListAsync();
        var requests = await _context.ServiceRequests.AsNoTracking().Where(request => request.ResidentId == resident.Id).OrderByDescending(request => request.CreatedAt).ToListAsync();
        return View(new ResidentPortalViewModel { Resident = resident, Bills = bills, ServiceRequests = requests });
    }

    [HttpGet]
    public async Task<IActionResult> Bill(int id)
    {
        var resident = await CurrentResidentAsync();
        if (resident is null) return Forbid();
        var billOwner = await _context.Bills.AsNoTracking()
            .Where(bill => bill.Id == id)
            .Join(_context.WaterConnections, bill => bill.WaterConnectionId, connection => connection.Id, (bill, connection) => new { bill, connection.ResidentId })
            .SingleOrDefaultAsync();
        if (billOwner is null) return NotFound();
        if (billOwner.ResidentId != resident.Id) return Forbid();
        return View(billOwner.bill);
    }

    private async Task<WaterSupply.Domain.Entities.Resident?> CurrentResidentAsync()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _context.Residents.AsNoTracking().SingleOrDefaultAsync(item => item.IdentityUserId == identityUserId);
    }
}
