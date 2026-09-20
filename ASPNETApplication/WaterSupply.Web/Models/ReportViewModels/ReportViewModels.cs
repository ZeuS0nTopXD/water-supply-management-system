using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models.ReportViewModels;

public sealed class OutstandingBillsReportViewModel
{
    public string? Status { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public List<OutstandingBillRowViewModel> Rows { get; set; } = [];
    public decimal TotalOutstanding => Rows.Sum(row => row.OutstandingAmount);
}

public sealed class OutstandingBillRowViewModel
{
    public int BillId { get; init; }
    public int WaterConnectionId { get; init; }
    public DateOnly BillingPeriodEnd { get; init; }
    public DateOnly DueDate { get; init; }
    public decimal OutstandingAmount { get; init; }
    public BillStatus Status { get; init; }
}

public sealed class ConsumptionReportViewModel
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public List<ConsumptionRowViewModel> Rows { get; set; } = [];
    public decimal TotalConsumption => Rows.Sum(row => row.Consumption);
}

public sealed class ConsumptionRowViewModel
{
    public int WaterConnectionId { get; init; }
    public string ConnectionNumber { get; init; } = string.Empty;
    public DateOnly ReadingDate { get; init; }
    public decimal Consumption { get; init; }
}

public sealed class PaymentsReportViewModel
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public List<PaymentRowViewModel> Rows { get; set; } = [];
    public decimal TotalCollected => Rows.Sum(row => row.Amount);
}

public sealed class PaymentRowViewModel
{
    public int PaymentId { get; init; }
    public int BillId { get; init; }
    public decimal Amount { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public DateTime PaymentDate { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;
}
