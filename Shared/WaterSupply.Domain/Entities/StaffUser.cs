namespace WaterSupply.Domain.Entities;

public sealed class StaffUser : Person
{
    public StaffUser(int id, string fullName, string email, string phone, string address, string role)
        : base(id, fullName, email, phone, address)
    {
        Role = string.IsNullOrWhiteSpace(role) ? throw new ArgumentException("Role is required.", nameof(role)) : role.Trim();
    }

    private StaffUser()
    {
    }

    public string Role { get; private set; } = string.Empty;
}
