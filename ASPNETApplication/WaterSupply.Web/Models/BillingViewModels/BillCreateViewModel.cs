using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.BillingViewModels;

public sealed class BillCreateViewModel
{
    [Range(1, int.MaxValue)]
    public int WaterConnectionId { get; set; }

    [Required]
    public DateOnly BillingPeriodStart { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    [Required]
    public DateOnly BillingPeriodEnd { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Range(0, double.MaxValue)]
    public decimal UnitsConsumed { get; set; }

    [Range(0, double.MaxValue)]
    public decimal RatePerUnit { get; set; }

    [Range(0, double.MaxValue)]
    public decimal FixedCharge { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; }

    [Required]
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(15));
}

public sealed class PaymentCreateViewModel
{
    [Range(1, int.MaxValue)]
    public int BillId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public WaterSupply.Domain.Enums.PaymentMethod PaymentMethod { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.Now;
}
