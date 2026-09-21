using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class ServiceRequest
{
    private ServiceRequest()
    {
    }

    public ServiceRequest(int residentId, int? waterConnectionId, RequestType requestType, string description, int id = 0)
    {
        if (residentId <= 0) throw new DomainValidationException("A valid resident is required.");
        if (string.IsNullOrWhiteSpace(description)) throw new DomainValidationException("Description is required.");
        ResidentId = residentId;
        WaterConnectionId = waterConnectionId;
        RequestType = requestType;
        Description = description.Trim();
        CreatedAt = DateTime.UtcNow;
        Status = RequestStatus.Open;
        ServiceRequestId = id;
    }

    public int ServiceRequestId { get; private set; }
    public int ResidentId { get; private set; }
    public int? WaterConnectionId { get; private set; }
    public RequestType RequestType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public RequestStatus Status { get; private set; }
    public string? StaffNotes { get; private set; }

    public void ChangeStatus(RequestStatus status, string? notes = null)
    {
        Status = status;
        StaffNotes = notes?.Trim();
    }
}
