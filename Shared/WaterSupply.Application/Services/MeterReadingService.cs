using WaterSupply.Application.Abstractions;
using WaterSupply.Application.InMemory;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Application.Services;

public sealed class MeterReadingService : IMeterReadingService
{
    private readonly InMemoryWaterSupplyStore _store;

    public MeterReadingService(InMemoryWaterSupplyStore store) => _store = store;

    public MeterReading Record(int connectionId, DateOnly date, decimal previousReading, decimal currentReading)
    {
        if (_store.Readings.Any(reading => reading.WaterConnectionId == connectionId && reading.ReadingDate == date))
        {
            throw new DomainValidationException("A reading already exists for this connection and date.");
        }

        var reading = new MeterReading(connectionId, date, _store.Readings.Count + 1);
        reading.RecordReading(previousReading, currentReading);
        _store.Readings.Add(reading);
        return reading;
    }
}
