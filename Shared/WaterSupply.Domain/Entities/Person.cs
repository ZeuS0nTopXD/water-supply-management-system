using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public abstract class Person
{
    protected Person()
    {
    }

    protected Person(string fullName, string email, string phone, string address)
    {
        FullName = Required(fullName, nameof(fullName));
        Email = Required(email, nameof(email));
        Phone = Required(phone, nameof(phone));
        Address = Required(address, nameof(address));
    }

    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;

    public void UpdateContact(string fullName, string email, string phone, string address)
    {
        FullName = Required(fullName, nameof(fullName));
        Email = Required(email, nameof(email));
        Phone = Required(phone, nameof(phone));
        Address = Required(address, nameof(address));
    }

    protected static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainValidationException($"{name} is required.");
        return value.Trim();
    }
}
