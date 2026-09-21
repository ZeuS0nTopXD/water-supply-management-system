using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models;

public sealed class DashboardRequestViewModel
{
    public int ServiceRequestId { get; init; }
    public RequestType RequestType { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public RequestStatus Status { get; init; }
}
