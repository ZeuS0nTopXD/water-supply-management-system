using WaterSupply.Domain.Entities;

namespace WaterSupply.Web.Models;

public sealed class ResidentPortalViewModel
{
    public Resident Resident { get; init; } = null!;
    public List<Bill> Bills { get; init; } = [];
    public List<ServiceRequest> ServiceRequests { get; init; } = [];
}
