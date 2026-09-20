using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class Payment : Entity
{
    private Payment()
    {
    }

    public Payment(int id, int billId, decimal amount, PaymentMethod paymentMethod, DateTime paymentDate, string? referenceNumber = null)
        : base(id)
    {
        if (billId <= 0) throw new DomainValidationException("A valid bill is required.");
        if (amount <= 0) throw new DomainValidationException("Payment amount must be greater than zero.");
        BillId = billId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        PaymentDate = paymentDate;
        ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber) ? $"PAY-{Id:000000}" : referenceNumber.Trim();
    }

    public int BillId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;
}
