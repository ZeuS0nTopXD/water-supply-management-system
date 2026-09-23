using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models;

public sealed class ResidentPortalViewModel
{
    public string ResidentName { get; init; } = string.Empty;
    public List<ResidentPortalConnectionViewModel> Connections { get; init; } = [];
    public List<ResidentPortalReadingViewModel> Readings { get; init; } = [];
    public List<ResidentPortalBillViewModel> Bills { get; init; } = [];
    public List<ResidentPortalRequestViewModel> ServiceRequests { get; init; } = [];
    public List<ResidentPortalConnectionRequestViewModel> ConnectionRequests { get; init; } = [];
}

public sealed class ResidentPortalConnectionViewModel
{
    public string ConnectionNumber { get; init; } = string.Empty;
    public ConnectionType ConnectionType { get; init; }
    public string MeterNumber { get; init; } = string.Empty;
    public ConnectionStatus Status { get; init; }
}

public sealed class ResidentPortalReadingViewModel
{
    public string ConnectionNumber { get; init; } = string.Empty;
    public DateOnly ReadingDate { get; init; }
    public decimal Consumption { get; init; }
}

public sealed class ResidentPortalBillViewModel
{
    public string ConnectionNumber { get; init; } = string.Empty;
    public DateOnly BillDate { get; init; }
    public decimal TotalAmount { get; init; }
    public DateOnly DueDate { get; init; }
    public BillStatus Status { get; init; }
}

public sealed class ResidentPortalRequestViewModel
{
    public string? ConnectionNumber { get; init; }
    public RequestType RequestType { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public RequestStatus Status { get; init; }
}

public sealed class ResidentPortalConnectionRequestViewModel
{
    public ConnectionType RequestedType { get; init; }
    public string ServiceAddress { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public ConnectionRequestStatus Status { get; init; }
    public string? StaffNotes { get; init; }

    public string StatusLabel => Status switch
    {
        ConnectionRequestStatus.InReview => "In review",
        ConnectionRequestStatus.Approved => "Approved",
        ConnectionRequestStatus.Rejected => "Rejected",
        _ => "Pending"
    };
}
