using WaterSupply.Application.Reports;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Application.Abstractions;

public interface IResidentService
{
    Resident Create(string fullName, string email, string phone, string address, DateOnly registrationDate, string? identityUserId = null);
    IReadOnlyList<Resident> Search(string query);
    void Delete(int residentId);
}

public interface IWaterConnectionService
{
    WaterConnection Create(int residentId, string connectionNumber, ConnectionType connectionType, string meterNumber, DateOnly connectionDate);
    IReadOnlyList<WaterConnection> Search(string query);
}

public interface IMeterReadingService
{
    MeterReading Record(int waterConnectionId, DateOnly readingDate, decimal previousReading, decimal currentReading);
}

public interface IBillingService
{
    Bill GenerateBill(int waterConnectionId, DateOnly billingPeriodStart, DateOnly billingPeriodEnd, decimal unitsConsumed, decimal ratePerUnit, decimal fixedCharge, decimal taxAmount, DateOnly dueDate);
    Payment RecordPayment(int billId, decimal amount, PaymentMethod paymentMethod, DateTime paymentDate);
    IReadOnlyList<Bill> SearchBills(string? status, DateOnly? from, DateOnly? to);
    IReadOnlyList<Bill> GetBillsForResident(int residentId);
}

public interface IServiceRequestService
{
    ServiceRequest Create(int residentId, int? waterConnectionId, ServiceRequestType requestType, string description);
    IReadOnlyList<ServiceRequest> Search(string? status, int? residentId = null);
    void UpdateStatus(int requestId, ServiceRequestStatus status, string? staffNotes = null);
}

public interface IReportService
{
    ConsumptionReport GetConsumptionReport(DateOnly from, DateOnly to);
    PaymentCollectionReport GetPaymentCollectionReport(DateOnly from, DateOnly to);
    OutstandingBillsReport GetOutstandingBillsReport();
}
