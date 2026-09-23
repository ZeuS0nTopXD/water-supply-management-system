using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Resident")]
public sealed class ResidentPortalController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var resident = await FindCurrentResidentAsync();
        if (resident is null) return Forbid();

        var connections = await context.WaterConnections
            .AsNoTracking()
            .Where(connection => connection.ResidentId == resident.ResidentId)
            .OrderBy(connection => connection.ConnectionNumber)
            .Select(connection => new ResidentPortalConnectionViewModel
            {
                ConnectionNumber = connection.ConnectionNumber,
                ConnectionType = connection.ConnectionType,
                MeterNumber = connection.MeterNumber,
                Status = connection.Status
            })
            .ToListAsync();

        var connectionIds = await context.WaterConnections
            .AsNoTracking()
            .Where(connection => connection.ResidentId == resident.ResidentId)
            .Select(connection => connection.WaterConnectionId)
            .ToListAsync();

        var readings = await context.MeterReadings
            .AsNoTracking()
            .Where(reading => connectionIds.Contains(reading.WaterConnectionId))
            .Join(
                context.WaterConnections.AsNoTracking(),
                reading => reading.WaterConnectionId,
                connection => connection.WaterConnectionId,
                (reading, connection) => new ResidentPortalReadingViewModel
                {
                    ConnectionNumber = connection.ConnectionNumber,
                    ReadingDate = reading.ReadingDate,
                    Consumption = reading.Consumption
                })
            .OrderByDescending(reading => reading.ReadingDate)
            .ToListAsync();

        var bills = await context.Bills
            .AsNoTracking()
            .Where(bill => connectionIds.Contains(bill.WaterConnectionId))
            .Join(
                context.WaterConnections.AsNoTracking(),
                bill => bill.WaterConnectionId,
                connection => connection.WaterConnectionId,
                (bill, connection) => new ResidentPortalBillViewModel
                {
                    ConnectionNumber = connection.ConnectionNumber,
                    BillDate = bill.BillDate,
                    TotalAmount = bill.TotalAmount,
                    DueDate = bill.DueDate,
                    Status = bill.Status
                })
            .OrderByDescending(bill => bill.BillDate)
            .ToListAsync();

        var requests = await context.ServiceRequests
            .AsNoTracking()
            .Where(request => request.ResidentId == resident.ResidentId)
            .GroupJoin(
                context.WaterConnections.AsNoTracking(),
                request => request.WaterConnectionId,
                connection => (int?)connection.WaterConnectionId,
                (request, connectionGroup) => new { request, connectionGroup })
            .SelectMany(
                item => item.connectionGroup.DefaultIfEmpty(),
                (item, connection) => new ResidentPortalRequestViewModel
                {
                    ConnectionNumber = connection == null ? null : connection.ConnectionNumber,
                    RequestType = item.request.RequestType,
                    Description = item.request.Description,
                    CreatedAt = item.request.CreatedAt,
                    Status = item.request.Status
                })
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();

        var connectionRequests = await context.WaterConnectionRequests
            .AsNoTracking()
            .Where(request => request.ResidentId == resident.ResidentId)
            .OrderByDescending(request => request.CreatedAt)
            .Select(request => new ResidentPortalConnectionRequestViewModel
            {
                RequestedType = request.RequestedType,
                ServiceAddress = request.ServiceAddress,
                CreatedAt = request.CreatedAt,
                Status = request.Status,
                StaffNotes = request.StaffNotes
            })
            .ToListAsync();

        return View(new ResidentPortalViewModel
        {
            ResidentName = resident.FullName,
            Connections = connections,
            Readings = readings,
            Bills = bills,
            ServiceRequests = requests,
            ConnectionRequests = connectionRequests
        });
    }

    private Task<WaterSupply.Domain.Entities.Resident?> FindCurrentResidentAsync()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;
        return context.Residents.SingleOrDefaultAsync(resident =>
            (identityUserId != null && resident.IdentityUserId == identityUserId)
            || (email != null && resident.Email == email));
    }
}
