using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models;

public sealed class DashboardRequestViewModel
{
    public int ServiceRequestId { get; init; }
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
