using WaterSupply.Application.Abstractions;
using WaterSupply.Domain.Entities;

namespace WaterSupply.Application.Services;

public sealed class ResidentService : IResidentService
{
    private readonly IRepository<Resident> _residents;

    public ResidentService(IRepository<Resident> residents)
    {
        _residents = residents;
    }

    public Resident Create(string fullName, string email, string phone, string address, DateOnly registrationDate, string? identityUserId = null)
    {
        var resident = new Resident(0, fullName, email, phone, address, registrationDate, identityUserId);
        _residents.Add(resident);
        return resident;
    }

    public IReadOnlyList<Resident> Search(string query)
    {
        var normalized = query?.Trim() ?? string.Empty;
        if (normalized.Length == 0) return _residents.GetAll();
        return _residents.GetAll().Where(resident => resident.FullName.Contains(normalized, StringComparison.OrdinalIgnoreCase)
            || resident.Email.Contains(normalized, StringComparison.OrdinalIgnoreCase)
            || resident.Phone.Contains(normalized, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public void Delete(int residentId) => _residents.Delete(residentId);
}
