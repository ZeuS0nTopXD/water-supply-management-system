using WaterSupply.Application.Abstractions;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Application.Services;

public sealed class ServiceRequestService : IServiceRequestService
{
    private readonly IRepository<ServiceRequest> _requests;

    public ServiceRequestService(IRepository<ServiceRequest> requests)
    {
        _requests = requests;
    }

    public ServiceRequest Create(int residentId, int? waterConnectionId, ServiceRequestType requestType, string description)
    {
        var request = ServiceRequest.Create(residentId, waterConnectionId, requestType, description, DateTime.UtcNow);
        _requests.Add(request);
        return request;
    }

    public IReadOnlyList<ServiceRequest> Search(string? status, int? residentId = null)
    {
        var query = _requests.GetAll().AsEnumerable();
        if (residentId.HasValue) query = query.Where(request => request.ResidentId == residentId.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ServiceRequestStatus>(status, true, out var parsedStatus)) query = query.Where(request => request.Status == parsedStatus);
        return query.OrderByDescending(request => request.CreatedAt).ToList();
    }

    public void UpdateStatus(int requestId, ServiceRequestStatus status, string? staffNotes = null)
    {
        var request = _requests.GetById(requestId) ?? throw new KeyNotFoundException($"Service request {requestId} was not found.");
        request.UpdateStatus(status, staffNotes);
        _requests.Update(request);
    }
}
