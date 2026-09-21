using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models.ReportViewModels;

public sealed class BillReportViewModel
{
    public string? Search { get; set; }
    public BillStatus? Status { get; set; }
    public List<BillReportRowViewModel> Rows { get; set; } = [];
    public decimal TotalAmount => Rows.Sum(row => row.TotalAmount);
}

public sealed class BillReportRowViewModel
{
    public int BillId { get; init; }
    public int WaterConnectionId { get; init; }
    public DateOnly BillDate { get; init; }
    public decimal TotalAmount { get; init; }
    public DateOnly DueDate { get; init; }
    public BillStatus Status { get; init; }
}
