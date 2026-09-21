using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class Bill
{
    private Bill()
    {
    }

    public Bill(int waterConnectionId, int meterReadingId, DateOnly billDate, int unitsConsumed, decimal ratePerUnit, int id = 0)
    {
        if (waterConnectionId <= 0 || meterReadingId <= 0) throw new DomainValidationException("A valid connection and reading are required.");
        if (unitsConsumed < 0 || ratePerUnit < 0) throw new DomainValidationException("Bill values cannot be negative.");
        WaterConnectionId = waterConnectionId;
        MeterReadingId = meterReadingId;
        BillDate = billDate;
        UnitsConsumed = unitsConsumed;
        RatePerUnit = ratePerUnit;
        DueDate = billDate.AddDays(30);
        Status = BillStatus.Unpaid;
        BillId = id;
    }

    public int BillId { get; private set; }
    public int WaterConnectionId { get; private set; }
    public int MeterReadingId { get; private set; }
    public DateOnly BillDate { get; private set; }
    public int UnitsConsumed { get; private set; }
    public decimal RatePerUnit { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public BillStatus Status { get; private set; }

    public void CalculateTotal() => TotalAmount = decimal.Round(UnitsConsumed * RatePerUnit, 2);
}
