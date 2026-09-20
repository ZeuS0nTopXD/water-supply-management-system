namespace WaterSupply.Domain.Entities;

public sealed class Resident : Person
{
    public Resident(int id, string fullName, string email, string phone, string address, DateOnly registrationDate, string? identityUserId = null, bool isActive = true)
        : base(id, fullName, email, phone, address)
    {
        RegistrationDate = registrationDate;
        IdentityUserId = identityUserId;
        IsActive = isActive;
    }

    private Resident()
    {
    }

    public string? IdentityUserId { get; private set; }
    public DateOnly RegistrationDate { get; private set; }
    public bool IsActive { get; private set; }

    public void LinkIdentityUser(string identityUserId)
    {
        IdentityUserId = string.IsNullOrWhiteSpace(identityUserId) ? throw new ArgumentException("Identity user ID is required.", nameof(identityUserId)) : identityUserId;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
