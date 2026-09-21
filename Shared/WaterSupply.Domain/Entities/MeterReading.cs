using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class MeterReading
{
    private MeterReading()
    {
    }

    public MeterReading(int waterConnectionId, DateOnly readingDate, int id = 0)
    {
        if (waterConnectionId <= 0) throw new DomainValidationException("A valid water connection is required.");
        WaterConnectionId = waterConnectionId;
        ReadingDate = readingDate;
        MeterReadingId = id;
    }

    public int MeterReadingId { get; private set; }
    public int WaterConnectionId { get; private set; }
    public DateOnly ReadingDate { get; private set; }
    public decimal PreviousReading { get; private set; }
    public decimal CurrentReading { get; private set; }
    public decimal Consumption { get; private set; }

    public void RecordReading(decimal previousReading, decimal currentReading)
    {
        if (previousReading < 0 || currentReading < 0) throw new DomainValidationException("Meter readings cannot be negative.");
        if (currentReading < previousReading) throw new DomainValidationException("Current reading cannot be lower than previous reading.");
        PreviousReading = previousReading;
        CurrentReading = currentReading;
        Consumption = currentReading - previousReading;
    }
}
