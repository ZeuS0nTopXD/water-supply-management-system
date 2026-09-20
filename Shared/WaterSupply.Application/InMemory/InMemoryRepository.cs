using WaterSupply.Application.Abstractions;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Application.InMemory;

public class InMemoryRepository<T> : IRepository<T> where T : Entity
{
    protected readonly Dictionary<int, T> Items = new();
    private readonly Func<T, int> _keySelector;

    public InMemoryRepository(Func<T, int> keySelector)
    {
        _keySelector = keySelector;
    }

    public virtual IReadOnlyList<T> GetAll() => Items.Values.OrderBy(entity => entity.Id).ToList();

    public virtual T? GetById(int id) => Items.GetValueOrDefault(id);

    public virtual void Add(T entity)
    {
        var key = _keySelector(entity);
        if (Items.ContainsKey(key)) throw new DomainValidationException($"An entity with ID {key} already exists.");
        Items.Add(key, entity);
    }

    public virtual void Update(T entity)
    {
        var key = _keySelector(entity);
        if (!Items.ContainsKey(key)) throw new KeyNotFoundException($"Entity {key} was not found.");
        Items[key] = entity;
    }

    public virtual void Delete(int id) => Items.Remove(id);
}

public sealed class InMemoryMeterReadingRepository : InMemoryRepository<MeterReading>
{
    public InMemoryMeterReadingRepository() : base(reading => reading.Id)
    {
    }

    public override void Add(MeterReading entity)
    {
        if (Items.Values.Any(reading => reading.WaterConnectionId == entity.WaterConnectionId && reading.ReadingDate == entity.ReadingDate))
        {
            throw new DomainValidationException("A reading already exists for this connection and date.");
        }

        base.Add(entity);
    }
}
