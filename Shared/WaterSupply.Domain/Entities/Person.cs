using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public abstract class Person : Entity
{
    protected Person(int id, string fullName, string email, string phone, string address) : base(id)
    {
        FullName = Required(fullName, nameof(fullName));
        Email = Required(email, nameof(email));
        Phone = Required(phone, nameof(phone));
        Address = Required(address, nameof(address));
    }

    protected Person()
    {
    }

    public string FullName { get; protected set; } = string.Empty;
    public string Email { get; protected set; } = string.Empty;
    public string Phone { get; protected set; } = string.Empty;
    public string Address { get; protected set; } = string.Empty;

    public void UpdateContact(string fullName, string email, string phone, string address)
    {
        FullName = Required(fullName, nameof(fullName));
        Email = Required(email, nameof(email));
        Phone = Required(phone, nameof(phone));
        Address = Required(address, nameof(address));
    }

    private static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException($"{name} is required.");
        }

        return value.Trim();
    }
}
