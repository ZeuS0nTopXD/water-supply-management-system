namespace WaterSupply.Domain.Entities;

public sealed class Resident : Person
{
    private Resident()
    {
    }

    public Resident(int id, string fullName, string email, string phone, string address, DateOnly registrationDate, bool isActive = true, string? identityUserId = null)
        : base(fullName, email, phone, address)
    {
        ResidentId = id;
        RegistrationDate = registrationDate;
        IsActive = isActive;
        IdentityUserId = identityUserId;
    }

    public int ResidentId { get; private set; }
    public string? IdentityUserId { get; private set; }
    public DateOnly RegistrationDate { get; private set; }
    public bool IsActive { get; private set; }

    public void LinkIdentityUser(string identityUserId) => IdentityUserId = Required(identityUserId, nameof(identityUserId));
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
