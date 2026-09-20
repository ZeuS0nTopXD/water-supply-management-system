namespace WaterSupply.Web.Models;

public sealed class DashboardSummaryViewModel
{
    public int ResidentCount { get; set; }
    public int ActiveConnectionCount { get; set; }
    public decimal CurrentMonthConsumption { get; set; }
    public decimal UnpaidBillAmount { get; set; }
    public decimal CollectionAmount { get; set; }
    public int OpenServiceRequestCount { get; set; }
}
