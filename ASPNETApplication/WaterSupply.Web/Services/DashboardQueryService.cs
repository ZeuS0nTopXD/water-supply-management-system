using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models;

namespace WaterSupply.Web.Services;

public sealed class DashboardQueryService(ApplicationDbContext context)
{
    public async Task<DashboardSummaryViewModel> GetSummaryAsync(DateOnly month)
    {
        var firstDay = new DateOnly(month.Year, month.Month, 1);
        var nextMonth = firstDay.AddMonths(1);

        return new DashboardSummaryViewModel
        {
            SelectedMonth = firstDay,
            ResidentCount = await context.Residents.CountAsync(),
            ActiveConnectionCount = await context.WaterConnections.CountAsync(connection => connection.Status == ConnectionStatus.Active),
            CurrentMonthConsumption = await context.MeterReadings
                .Where(reading => reading.ReadingDate >= firstDay && reading.ReadingDate < nextMonth)
                .Select(reading => (decimal?)reading.Consumption)
                .SumAsync() ?? 0,
            UnpaidBillAmount = await context.Bills
                .Where(bill => bill.Status == BillStatus.Unpaid)
                .Select(bill => (decimal?)bill.TotalAmount)
                .SumAsync() ?? 0,
            OpenServiceRequestCount = await context.ServiceRequests
                .CountAsync(request => request.Status != RequestStatus.Closed),
            InProgressServiceRequestCount = await context.ServiceRequests
                .CountAsync(request => request.Status == RequestStatus.InProgress),
            ClosedServiceRequestCount = await context.ServiceRequests
                .CountAsync(request => request.Status == RequestStatus.Closed),
            PendingConnectionRequestCount = await context.WaterConnectionRequests
                .CountAsync(request => request.Status == ConnectionRequestStatus.Pending),
            RecentRequests = await context.ServiceRequests
                .AsNoTracking()
                .OrderByDescending(request => request.CreatedAt)
                .Take(5)
                .Select(request => new DashboardRequestViewModel
                {
                    ServiceRequestId = request.ServiceRequestId,
                    RequestType = request.RequestType,
                    Description = request.Description,
                    CreatedAt = request.CreatedAt,
                    Status = request.Status
                })
                .ToListAsync()
        };
    }
}
