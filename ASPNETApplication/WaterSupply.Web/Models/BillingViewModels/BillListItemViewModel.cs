using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models.BillingViewModels;

public sealed class BillListItemViewModel
{
    public int BillId { get; init; }
    public string ConnectionNumber { get; init; } = string.Empty;
    public DateOnly BillDate { get; init; }
    public decimal TotalAmount { get; init; }
    public DateOnly DueDate { get; init; }
    public BillStatus Status { get; init; }
}
