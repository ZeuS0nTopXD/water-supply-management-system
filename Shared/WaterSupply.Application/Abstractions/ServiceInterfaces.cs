using WaterSupply.Application.Reports;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Application.Abstractions;

public interface IResidentService
{
    Resident Create(string fullName, string email, string phone, string address, DateOnly registrationDate);
    IReadOnlyList<Resident> Search(string? query);
}

public interface IWaterConnectionService
{
    WaterConnection Create(int residentId, string connectionNumber, ConnectionType type, string meterNumber, DateOnly connectionDate);
    IReadOnlyList<WaterConnection> Search(string? query);
}

public interface IMeterReadingService
{
    MeterReading Record(int connectionId, DateOnly date, decimal previousReading, decimal currentReading);
}

public interface IBillingService
{
    Bill Generate(int connectionId, int readingId, DateOnly billDate, int units, decimal rate);
    IReadOnlyList<Bill> GetAll();
}

public interface IServiceRequestService
{
    ServiceRequest Create(int residentId, int? connectionId, RequestType type, string description);
    IReadOnlyList<ServiceRequest> Search(RequestStatus? status = null, int? residentId = null);
    void UpdateStatus(int requestId, RequestStatus status, string? notes = null);
}

public interface IReportService
{
    SummaryReport GetSummary();
}
