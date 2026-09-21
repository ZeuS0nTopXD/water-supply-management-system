using WaterSupply.Application.Abstractions;
using WaterSupply.Application.InMemory;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Application.Services;

public sealed class ServiceRequestService : IServiceRequestService
{
    private readonly InMemoryWaterSupplyStore _store;

    public ServiceRequestService(InMemoryWaterSupplyStore store) => _store = store;

    public ServiceRequest Create(int residentId, int? connectionId, RequestType type, string description)
    {
        var request = new ServiceRequest(residentId, connectionId, type, description, _store.ServiceRequests.Count + 1);
        _store.ServiceRequests.Add(request);
        return request;
    }

    public IReadOnlyList<ServiceRequest> Search(RequestStatus? status = null, int? residentId = null)
    {
        var query = _store.ServiceRequests.AsEnumerable();
        if (status.HasValue) query = query.Where(request => request.Status == status.Value);
        if (residentId.HasValue) query = query.Where(request => request.ResidentId == residentId.Value);
        return query.OrderByDescending(request => request.CreatedAt).ToList();
    }

    public void UpdateStatus(int requestId, RequestStatus status, string? notes = null)
    {
        var request = _store.ServiceRequests.Single(item => item.ServiceRequestId == requestId);
        request.ChangeStatus(status, notes);
    }
}
