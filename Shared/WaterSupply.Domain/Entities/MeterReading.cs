using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class MeterReading : Entity
{
    private MeterReading()
    {
    }

    private MeterReading(int id, int waterConnectionId, DateOnly readingDate, decimal previousReading, decimal currentReading)
        : base(id)
    {
        if (waterConnectionId <= 0) throw new DomainValidationException("A valid water connection is required.");
        if (previousReading < 0 || currentReading < 0) throw new DomainValidationException("Meter readings cannot be negative.");
        if (currentReading < previousReading) throw new DomainValidationException("Current reading cannot be lower than previous reading.");
        WaterConnectionId = waterConnectionId;
        ReadingDate = readingDate;
        PreviousReading = previousReading;
        CurrentReading = currentReading;
        Consumption = currentReading - previousReading;
    }

    public int WaterConnectionId { get; private set; }
    public DateOnly ReadingDate { get; private set; }
    public decimal PreviousReading { get; private set; }
    public decimal CurrentReading { get; private set; }
    public decimal Consumption { get; private set; }

    public static MeterReading Create(int waterConnectionId, DateOnly readingDate, decimal previousReading, decimal currentReading, int id = 0)
        => new(id, waterConnectionId, readingDate, previousReading, currentReading);
}
