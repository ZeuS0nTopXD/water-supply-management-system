namespace WaterSupply.Application.Reports;

public sealed record ConsumptionReportRow(int WaterConnectionId, string ConnectionNumber, decimal UnitsConsumed);
public sealed record ConsumptionReport(IReadOnlyList<ConsumptionReportRow> Rows, decimal TotalConsumption);
public sealed record PaymentCollectionReportRow(int PaymentId, int BillId, decimal Amount, DateTime PaymentDate);
public sealed record PaymentCollectionReport(IReadOnlyList<PaymentCollectionReportRow> Rows, decimal TotalCollected);
public sealed record OutstandingBillReportRow(int BillId, int WaterConnectionId, decimal OutstandingAmount, DateOnly DueDate, string Status);
public sealed record OutstandingBillsReport(IReadOnlyList<OutstandingBillReportRow> Rows, decimal TotalOutstanding);
