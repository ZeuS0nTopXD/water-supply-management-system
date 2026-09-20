using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ReportViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> OutstandingBills(string? status, DateOnly? from, DateOnly? to)
    {
        var query = _context.Bills.AsNoTracking();
        if (Enum.TryParse<BillStatus>(status, true, out var selectedStatus)) query = query.Where(bill => bill.Status == selectedStatus);
        if (from.HasValue) query = query.Where(bill => bill.BillingPeriodEnd >= from.Value);
        if (to.HasValue) query = query.Where(bill => bill.BillingPeriodEnd <= to.Value);

        var rows = await query.OrderByDescending(bill => bill.BillingPeriodEnd)
            .Select(bill => new OutstandingBillRowViewModel
            {
                BillId = bill.Id,
                WaterConnectionId = bill.WaterConnectionId,
                BillingPeriodEnd = bill.BillingPeriodEnd,
                DueDate = bill.DueDate,
                OutstandingAmount = bill.TotalAmount - bill.PaidAmount,
                Status = bill.Status
            }).ToListAsync();

        return View(new OutstandingBillsReportViewModel { Status = status, From = from, To = to, Rows = rows });
    }

    [HttpGet]
    public async Task<IActionResult> Consumption(DateOnly? from, DateOnly? to)
    {
        var query = from reading in _context.MeterReadings.AsNoTracking()
                    join connection in _context.WaterConnections.AsNoTracking() on reading.WaterConnectionId equals connection.Id
                    select new { reading, connection };
        if (from.HasValue) query = query.Where(item => item.reading.ReadingDate >= from.Value);
        if (to.HasValue) query = query.Where(item => item.reading.ReadingDate <= to.Value);

        var rows = await query.OrderByDescending(item => item.reading.ReadingDate)
            .Select(item => new ConsumptionRowViewModel
            {
                WaterConnectionId = item.reading.WaterConnectionId,
                ConnectionNumber = item.connection.ConnectionNumber,
                ReadingDate = item.reading.ReadingDate,
                Consumption = item.reading.Consumption
            }).ToListAsync();
        return View(new ConsumptionReportViewModel { From = from, To = to, Rows = rows });
    }

    [HttpGet]
    public async Task<IActionResult> Payments(DateOnly? from, DateOnly? to)
    {
        var query = _context.Payments.AsNoTracking();
        if (from.HasValue) query = query.Where(payment => payment.PaymentDate >= from.Value.ToDateTime(TimeOnly.MinValue));
        if (to.HasValue) query = query.Where(payment => payment.PaymentDate < to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));

        var rows = await query.OrderByDescending(payment => payment.PaymentDate)
            .Select(payment => new PaymentRowViewModel
            {
                PaymentId = payment.Id,
                BillId = payment.BillId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentDate = payment.PaymentDate,
                ReferenceNumber = payment.ReferenceNumber
            }).ToListAsync();
        return View(new PaymentsReportViewModel { From = from, To = to, Rows = rows });
    }
}
