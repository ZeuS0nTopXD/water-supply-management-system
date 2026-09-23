using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Domain.Entities;

public sealed class WaterConnectionRequest
{
    private WaterConnectionRequest()
    {
    }

    public WaterConnectionRequest(
        int residentId,
        ConnectionType requestedType,
        string serviceAddress,
        string? notes = null,
        int id = 0)
    {
        if (residentId <= 0) throw new DomainValidationException("A valid resident is required.");
        if (string.IsNullOrWhiteSpace(serviceAddress)) throw new DomainValidationException("A service address is required.");

        WaterConnectionRequestId = id;
        ResidentId = residentId;
        RequestedType = requestedType;
        ServiceAddress = serviceAddress.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        CreatedAt = DateTime.UtcNow;
        Status = ConnectionRequestStatus.Pending;
    }

    public int WaterConnectionRequestId { get; private set; }
    public int ResidentId { get; private set; }
    public ConnectionType RequestedType { get; private set; }
    public string ServiceAddress { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ConnectionRequestStatus Status { get; private set; }
    public string? StaffNotes { get; private set; }

    public void ChangeStatus(ConnectionRequestStatus status, string? staffNotes = null)
    {
        Status = status;
        StaffNotes = string.IsNullOrWhiteSpace(staffNotes) ? null : staffNotes.Trim();
    }
}
