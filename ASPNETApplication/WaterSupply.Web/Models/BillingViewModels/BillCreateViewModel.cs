using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.BillingViewModels;

public sealed class BillCreateViewModel
{
    [Display(Name = "Water connection")]
    [Range(1, int.MaxValue)]
    public int WaterConnectionId { get; set; }

    [Display(Name = "Meter reading")]
    [Range(1, int.MaxValue)]
    public int MeterReadingId { get; set; }

    [Display(Name = "Bill date")]
    [Required, DataType(DataType.Date)]
    public DateOnly BillDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Units consumed")]
    [Range(0, int.MaxValue)]
    public int UnitsConsumed { get; set; }

    [Display(Name = "Rate per unit")]
    [Range(0, double.MaxValue)]
    public decimal RatePerUnit { get; set; }
}
