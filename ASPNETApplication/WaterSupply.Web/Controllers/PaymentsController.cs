using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Exceptions;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.BillingViewModels;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class PaymentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? billId)
    {
        var query = _context.Payments.AsNoTracking();
        if (billId.HasValue) query = query.Where(payment => payment.BillId == billId.Value);
        return View(await query.OrderByDescending(payment => payment.PaymentDate).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? billId)
    {
        await LoadBillsAsync();
        return View(new PaymentCreateViewModel { BillId = billId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentCreateViewModel model)
    {
        var bill = await _context.Bills.FindAsync(model.BillId);
        if (bill is null) ModelState.AddModelError(nameof(model.BillId), "The selected bill was not found.");
        if (bill is not null && model.Amount > bill.OutstandingAmount) ModelState.AddModelError(nameof(model.Amount), "Payment cannot exceed the outstanding balance.");
        if (!ModelState.IsValid)
        {
            await LoadBillsAsync();
            return View(model);
        }

        try
        {
            bill!.RecordPayment(model.Amount, model.PaymentMethod, model.PaymentDate);
            _context.Payments.Add(new Payment(0, bill.Id, model.Amount, model.PaymentMethod, model.PaymentDate));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { billId = bill.Id });
        }
        catch (DomainValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadBillsAsync();
            return View(model);
        }
    }

    private async Task LoadBillsAsync() => ViewBag.Bills = await _context.Bills.AsNoTracking().Where(bill => bill.Status != WaterSupply.Domain.Enums.BillStatus.Paid).OrderByDescending(bill => bill.BillingPeriodEnd).ToListAsync();
}
