using WaterSupply.Domain.Entities;

namespace WaterSupply.Application.Abstractions;

public interface IRepository<T> where T : Entity
{
    IReadOnlyList<T> GetAll();
    T? GetById(int id);
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}
