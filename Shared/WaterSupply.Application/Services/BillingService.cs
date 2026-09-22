using WaterSupply.Application.Abstractions;
using WaterSupply.Application.InMemory;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Application.Services;

public sealed class BillingService : IBillingService
{
    private readonly InMemoryWaterSupplyStore _store;

    public BillingService(InMemoryWaterSupplyStore store) => _store = store;

    public Bill Generate(int connectionId, int readingId, DateOnly billDate, int units, decimal rate)
    {
        if (!_store.Connections.Any(connection => connection.WaterConnectionId == connectionId))
        {
            throw new DomainValidationException("A valid water connection is required.");
        }

        var reading = _store.Readings.SingleOrDefault(item => item.MeterReadingId == readingId);
        if (reading is null)
        {
            throw new DomainValidationException("A valid meter reading is required.");
        }

        if (reading.WaterConnectionId != connectionId)
        {
            throw new DomainValidationException("The meter reading must belong to the selected connection.");
        }

        if (_store.Bills.Any(bill => bill.WaterConnectionId == connectionId && bill.BillDate == billDate))
        {
            throw new DomainValidationException("A bill already exists for this connection and date.");
        }

        var bill = new Bill(connectionId, readingId, billDate, units, rate, _store.Bills.Count + 1);
        bill.CalculateTotal();
        _store.Bills.Add(bill);
        return bill;
    }

    public IReadOnlyList<Bill> GetAll() => _store.Bills;
}
