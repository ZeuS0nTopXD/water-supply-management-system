namespace WaterSupply.Web.Models;

public sealed class DashboardSummaryViewModel
{
    public DateOnly SelectedMonth { get; set; }
    public string SelectedMonthValue => SelectedMonth.ToString("yyyy-MM");
    public int ResidentCount { get; set; }
    public int ActiveConnectionCount { get; set; }
    public decimal CurrentMonthConsumption { get; set; }
    public decimal UnpaidBillAmount { get; set; }
    public int OpenServiceRequestCount { get; set; }
    public int InProgressServiceRequestCount { get; set; }
    public int ClosedServiceRequestCount { get; set; }
    public List<DashboardRequestViewModel> RecentRequests { get; set; } = [];
}
