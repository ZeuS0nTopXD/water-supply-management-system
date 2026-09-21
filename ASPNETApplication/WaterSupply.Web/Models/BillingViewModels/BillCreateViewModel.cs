using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.BillingViewModels;

public sealed class BillCreateViewModel
{
    [Range(1, int.MaxValue)]
    public int WaterConnectionId { get; set; }

    [Range(1, int.MaxValue)]
    public int MeterReadingId { get; set; }

    [Required]
    public DateOnly BillDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Range(0, int.MaxValue)]
    public int UnitsConsumed { get; set; }

    [Range(0, double.MaxValue)]
    public decimal RatePerUnit { get; set; }
}
