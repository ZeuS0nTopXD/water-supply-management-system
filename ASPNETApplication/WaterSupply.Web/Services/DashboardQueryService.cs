using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models;

namespace WaterSupply.Web.Services;

public sealed class DashboardQueryService
{
    private readonly ApplicationDbContext _context;

    public DashboardQueryService(ApplicationDbContext context) => _context = context;

    public async Task<DashboardSummaryViewModel> GetSummaryAsync(DateOnly month)
    {
        var firstDay = new DateOnly(month.Year, month.Month, 1);
        var nextMonth = firstDay.AddMonths(1);
        var firstDayDateTime = firstDay.ToDateTime(TimeOnly.MinValue);
        var nextMonthDateTime = nextMonth.ToDateTime(TimeOnly.MinValue);

        return new DashboardSummaryViewModel
        {
            ResidentCount = await _context.Residents.CountAsync(),
            ActiveConnectionCount = await _context.WaterConnections.CountAsync(connection => connection.Status == ConnectionStatus.Active),
            CurrentMonthConsumption = await _context.MeterReadings
                .Where(reading => reading.ReadingDate >= firstDay && reading.ReadingDate < nextMonth)
                .Select(reading => (decimal?)reading.Consumption)
                .SumAsync() ?? 0,
            UnpaidBillAmount = await _context.Bills
                .Where(bill => bill.Status != BillStatus.Paid)
                .Select(bill => (decimal?)(bill.TotalAmount - bill.PaidAmount))
                .SumAsync() ?? 0,
            CollectionAmount = await _context.Payments
                .Where(payment => payment.PaymentDate >= firstDayDateTime && payment.PaymentDate < nextMonthDateTime)
                .Select(payment => (decimal?)payment.Amount)
                .SumAsync() ?? 0,
            OpenServiceRequestCount = await _context.ServiceRequests
                .CountAsync(request => request.Status == ServiceRequestStatus.Open || request.Status == ServiceRequestStatus.InProgress)
        };
    }
}
