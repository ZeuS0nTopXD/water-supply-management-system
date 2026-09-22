using WaterSupply.Application.Abstractions;
using WaterSupply.Application.InMemory;
using WaterSupply.Domain.Entities;

namespace WaterSupply.Application.Services;

public sealed class BillingService : IBillingService
{
    private readonly InMemoryWaterSupplyStore _store;

    public BillingService(InMemoryWaterSupplyStore store) => _store = store;

    public Bill Generate(int connectionId, int readingId, DateOnly billDate, int units, decimal rate)
    {
        var bill = new Bill(connectionId, readingId, billDate, units, rate, _store.Bills.Count + 1);
        bill.CalculateTotal();
        _store.Bills.Add(bill);
        return bill;
    }

    public IReadOnlyList<Bill> GetAll() => _store.Bills;
}
