using WaterSupply.Application.Abstractions;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Application.Services;

public sealed class BillingService : IBillingService
{
    private readonly IRepository<Bill> _bills;
    private readonly IRepository<Payment> _payments;
    private readonly IRepository<WaterConnection> _connections;

    public BillingService(IRepository<Bill> bills, IRepository<Payment> payments, IRepository<WaterConnection> connections)
    {
        _bills = bills;
        _payments = payments;
        _connections = connections;
    }

    public Bill GenerateBill(int waterConnectionId, DateOnly billingPeriodStart, DateOnly billingPeriodEnd, decimal unitsConsumed, decimal ratePerUnit, decimal fixedCharge, decimal taxAmount, DateOnly dueDate)
    {
        var bill = Bill.Create(0, billingPeriodStart, billingPeriodEnd, unitsConsumed, ratePerUnit, fixedCharge, taxAmount, dueDate, waterConnectionId);
        if (_bills.GetAll().Any(existing => existing.WaterConnectionId == waterConnectionId && existing.BillingPeriodStart == billingPeriodStart && existing.BillingPeriodEnd == billingPeriodEnd))
        {
            throw new InvalidOperationException("A bill already exists for this connection and billing period.");
        }

        _bills.Add(bill);
        return bill;
    }

    public Payment RecordPayment(int billId, decimal amount, PaymentMethod paymentMethod, DateTime paymentDate)
    {
        var bill = _bills.GetById(billId) ?? throw new KeyNotFoundException($"Bill {billId} was not found.");
        bill.RecordPayment(amount, paymentMethod, paymentDate);
        var payment = new Payment(0, billId, amount, paymentMethod, paymentDate);
        _payments.Add(payment);
        _bills.Update(bill);
        return payment;
    }

    public IReadOnlyList<Bill> SearchBills(string? status, DateOnly? from, DateOnly? to)
    {
        var query = _bills.GetAll().AsEnumerable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BillStatus>(status, true, out var parsedStatus)) query = query.Where(bill => bill.Status == parsedStatus);
        if (from.HasValue) query = query.Where(bill => bill.BillingPeriodStart >= from.Value);
        if (to.HasValue) query = query.Where(bill => bill.BillingPeriodEnd <= to.Value);
        return query.OrderByDescending(bill => bill.BillingPeriodEnd).ToList();
    }

    public IReadOnlyList<Bill> GetBillsForResident(int residentId)
    {
        var connectionIds = _connections.GetAll().Where(connection => connection.ResidentId == residentId).Select(connection => connection.Id).ToHashSet();
        return _bills.GetAll().Where(bill => connectionIds.Contains(bill.WaterConnectionId)).ToList();
    }
}
