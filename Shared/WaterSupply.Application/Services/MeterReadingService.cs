using WaterSupply.Application.Abstractions;
using WaterSupply.Domain.Entities;

namespace WaterSupply.Application.Services;

public sealed class MeterReadingService : IMeterReadingService
{
    private readonly IRepository<MeterReading> _readings;

    public MeterReadingService(IRepository<MeterReading> readings)
    {
        _readings = readings;
    }

    public MeterReading Record(int waterConnectionId, DateOnly readingDate, decimal previousReading, decimal currentReading)
    {
        var reading = MeterReading.Create(waterConnectionId, readingDate, previousReading, currentReading);
        _readings.Add(reading);
        return reading;
    }
}
