using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class Bill : Entity
{
    private Bill()
    {
    }

    private Bill(int id, int waterConnectionId, DateOnly billingPeriodStart, DateOnly billingPeriodEnd, decimal unitsConsumed, decimal ratePerUnit, decimal fixedCharge, decimal taxAmount, DateOnly dueDate)
        : base(id)
    {
        if (waterConnectionId <= 0) throw new DomainValidationException("A valid water connection is required.");
        if (billingPeriodEnd < billingPeriodStart) throw new DomainValidationException("Billing period is invalid.");
        if (unitsConsumed < 0 || ratePerUnit < 0 || fixedCharge < 0 || taxAmount < 0) throw new DomainValidationException("Bill amounts cannot be negative.");
        if (dueDate < billingPeriodEnd) throw new DomainValidationException("Due date cannot precede the billing period end.");
        WaterConnectionId = waterConnectionId;
        BillingPeriodStart = billingPeriodStart;
        BillingPeriodEnd = billingPeriodEnd;
        UnitsConsumed = unitsConsumed;
        RatePerUnit = ratePerUnit;
        FixedCharge = fixedCharge;
        TaxAmount = taxAmount;
        TotalAmount = decimal.Round(unitsConsumed * ratePerUnit + fixedCharge + taxAmount, 2);
        DueDate = dueDate;
        Status = BillStatus.Unpaid;
    }

    public int WaterConnectionId { get; private set; }
    public DateOnly BillingPeriodStart { get; private set; }
    public DateOnly BillingPeriodEnd { get; private set; }
    public decimal UnitsConsumed { get; private set; }
    public decimal RatePerUnit { get; private set; }
    public decimal FixedCharge { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public BillStatus Status { get; private set; }
    public decimal OutstandingAmount => decimal.Round(TotalAmount - PaidAmount, 2);

    public static Bill Create(int id, DateOnly billingPeriodStart, DateOnly billingPeriodEnd, decimal unitsConsumed, decimal ratePerUnit, decimal fixedCharge, decimal taxAmount, DateOnly dueDate, int waterConnectionId = 1)
        => new(id, waterConnectionId, billingPeriodStart, billingPeriodEnd, unitsConsumed, ratePerUnit, fixedCharge, taxAmount, dueDate);

    public void RecordPayment(decimal amount, PaymentMethod paymentMethod, DateTime paymentDate)
    {
        if (amount <= 0) throw new DomainValidationException("Payment amount must be greater than zero.");
        if (amount > OutstandingAmount) throw new DomainValidationException("Payment cannot exceed the outstanding bill balance.");
        PaidAmount = decimal.Round(PaidAmount + amount, 2);
        Status = PaidAmount == TotalAmount ? BillStatus.Paid : BillStatus.PartiallyPaid;
    }

    public void MarkOverdue(DateOnly today)
    {
        if (Status != BillStatus.Paid && today > DueDate) Status = BillStatus.Overdue;
    }
}
