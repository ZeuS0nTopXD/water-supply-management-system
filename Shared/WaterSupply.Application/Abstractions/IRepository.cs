namespace WaterSupply.Application.Abstractions;

public interface IRepository<T>
{
    IReadOnlyList<T> GetAll();
    void Add(T item);
}
