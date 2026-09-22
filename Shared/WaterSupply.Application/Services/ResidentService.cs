using WaterSupply.Application.Abstractions;
using WaterSupply.Application.InMemory;
using WaterSupply.Domain.Entities;

namespace WaterSupply.Application.Services;

public sealed class ResidentService : IResidentService
{
    private readonly InMemoryWaterSupplyStore _store;

    public ResidentService(InMemoryWaterSupplyStore store) => _store = store;

    public Resident Create(string fullName, string email, string phone, string address, DateOnly registrationDate)
    {
        var resident = new Resident(_store.Residents.Count + 1, fullName, email, phone, address, registrationDate);
        _store.Residents.Add(resident);
        return resident;
    }

    public IReadOnlyList<Resident> Search(string? query)
    {
        var normalized = query?.Trim() ?? string.Empty;
        if (normalized.Length == 0) return _store.Residents;
        return _store.Residents.Where(resident => resident.FullName.Contains(normalized, StringComparison.OrdinalIgnoreCase)
            || resident.Email.Contains(normalized, StringComparison.OrdinalIgnoreCase)
            || resident.Phone.Contains(normalized, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
