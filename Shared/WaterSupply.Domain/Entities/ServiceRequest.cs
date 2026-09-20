using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class ServiceRequest : Entity
{
    private ServiceRequest()
    {
    }

    private ServiceRequest(int id, int residentId, int? waterConnectionId, ServiceRequestType requestType, string description, DateTime createdAt)
        : base(id)
    {
        if (residentId <= 0) throw new DomainValidationException("A valid resident is required.");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainValidationException("Description is required.");
        ResidentId = residentId;
        WaterConnectionId = waterConnectionId;
        RequestType = requestType;
        Description = description.Trim();
        CreatedAt = createdAt;
        Status = ServiceRequestStatus.Open;
    }

    public int ResidentId { get; private set; }
    public int? WaterConnectionId { get; private set; }
    public ServiceRequestType RequestType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public ServiceRequestStatus Status { get; private set; }
    public string? StaffNotes { get; private set; }

    public static ServiceRequest Create(int residentId, int? waterConnectionId, ServiceRequestType requestType, string description, DateTime createdAt, int id = 0)
        => new(id, residentId, waterConnectionId, requestType, description, createdAt);

    public void UpdateStatus(ServiceRequestStatus status, string? staffNotes = null, DateTime? resolvedAt = null)
    {
        Status = status;
        StaffNotes = staffNotes?.Trim();
        ResolvedAt = status == ServiceRequestStatus.Resolved ? resolvedAt ?? DateTime.UtcNow : null;
    }
}
