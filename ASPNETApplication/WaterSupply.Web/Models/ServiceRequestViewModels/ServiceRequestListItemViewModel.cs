using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models.ServiceRequestViewModels;

public sealed class ServiceRequestListItemViewModel
{
    public int ServiceRequestId { get; init; }
    public int ResidentId { get; init; }
    public string ResidentName { get; init; } = string.Empty;
    public RequestType RequestType { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public RequestStatus Status { get; init; }

    public string RequestTypeLabel => RequestType switch
    {
        RequestType.NoSupply => "No supply",
        RequestType.Leak => "Leak",
        _ => "Other"
    };

    public string StatusLabel => Status switch
    {
        RequestStatus.InProgress => "In progress",
        RequestStatus.Closed => "Closed",
        _ => "Open"
    };
}
