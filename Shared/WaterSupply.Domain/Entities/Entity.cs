namespace WaterSupply.Domain.Entities;

public abstract class Entity
{
    protected Entity(int id = 0)
    {
        Id = id == 0 ? EntityIdGenerator.Next() : id;
    }

    protected Entity()
    {
    }

    public int Id { get; protected set; }
}

internal static class EntityIdGenerator
{
    private static int _next;

    public static int Next() => Interlocked.Increment(ref _next);
}
