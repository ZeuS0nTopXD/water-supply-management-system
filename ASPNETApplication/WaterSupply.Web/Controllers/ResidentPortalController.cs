using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Web.Data;

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
    public async Task<IActionResult> Bill(int id)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var resident = await _context.Residents.AsNoTracking().SingleOrDefaultAsync(item => item.IdentityUserId == identityUserId);
        if (resident is null) return Forbid();
        var billOwner = await _context.Bills.AsNoTracking()
            .Where(bill => bill.Id == id)
            .Join(_context.WaterConnections, bill => bill.WaterConnectionId, connection => connection.Id, (bill, connection) => new { bill, connection.ResidentId })
            .SingleOrDefaultAsync();
        if (billOwner is null) return NotFound();
        if (billOwner.ResidentId != resident.Id) return Forbid();
        return View(billOwner.bill);
    }
}
