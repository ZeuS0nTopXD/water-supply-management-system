namespace WaterSupply.Application.Reports;

public sealed record SummaryReport(int Residents, int ActiveConnections, decimal TotalConsumption, decimal TotalBillAmount, int OpenRequests);
