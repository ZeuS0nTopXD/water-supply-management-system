using WaterSupply.Application.Abstractions;
using WaterSupply.Application.Reports;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Application.Services;

public sealed class ReportService : IReportService
{
    private readonly IRepository<MeterReading> _readings;
    private readonly IRepository<WaterConnection> _connections;
    private readonly IRepository<Bill> _bills;
    private readonly IRepository<Payment> _payments;

    public ReportService(IRepository<MeterReading> readings, IRepository<WaterConnection> connections, IRepository<Bill> bills, IRepository<Payment> payments)
    {
        _readings = readings;
        _connections = connections;
        _bills = bills;
        _payments = payments;
    }

    public ConsumptionReport GetConsumptionReport(DateOnly from, DateOnly to)
    {
        var connectionNumbers = _connections.GetAll().ToDictionary(connection => connection.Id, connection => connection.ConnectionNumber);
        var rows = _readings.GetAll().Where(reading => reading.ReadingDate >= from && reading.ReadingDate <= to)
            .GroupBy(reading => reading.WaterConnectionId)
            .Select(group => new ConsumptionReportRow(group.Key, connectionNumbers.GetValueOrDefault(group.Key, "Unknown"), group.Sum(reading => reading.Consumption)))
            .OrderBy(row => row.ConnectionNumber)
            .ToList();
        return new ConsumptionReport(rows, rows.Sum(row => row.UnitsConsumed));
    }

    public PaymentCollectionReport GetPaymentCollectionReport(DateOnly from, DateOnly to)
    {
        var rows = _payments.GetAll().Where(payment => DateOnly.FromDateTime(payment.PaymentDate) >= from && DateOnly.FromDateTime(payment.PaymentDate) <= to)
            .Select(payment => new PaymentCollectionReportRow(payment.Id, payment.BillId, payment.Amount, payment.PaymentDate))
            .OrderByDescending(row => row.PaymentDate)
            .ToList();
        return new PaymentCollectionReport(rows, rows.Sum(row => row.Amount));
    }

    public OutstandingBillsReport GetOutstandingBillsReport()
    {
        var rows = _bills.GetAll().Where(bill => bill.Status is BillStatus.Unpaid or BillStatus.PartiallyPaid or BillStatus.Overdue)
            .Select(bill => new OutstandingBillReportRow(bill.Id, bill.WaterConnectionId, bill.OutstandingAmount, bill.DueDate, bill.Status.ToString()))
            .OrderBy(row => row.DueDate)
            .ToList();
        return new OutstandingBillsReport(rows, rows.Sum(row => row.OutstandingAmount));
    }
}
