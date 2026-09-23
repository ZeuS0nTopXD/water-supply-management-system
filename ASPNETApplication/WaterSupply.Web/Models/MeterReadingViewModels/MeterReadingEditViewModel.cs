using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.MeterReadingViewModels;

public sealed class MeterReadingEditViewModel
{
    [Display(Name = "Water connection")]
    [Range(1, int.MaxValue)]
    public int WaterConnectionId { get; set; }

    [Display(Name = "Reading date")]
    [Required, DataType(DataType.Date)]
    public DateOnly ReadingDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Previous reading")]
    [Range(0, double.MaxValue)]
    public decimal PreviousReading { get; set; }

    [Display(Name = "Current reading")]
    [Range(0, double.MaxValue)]
    public decimal CurrentReading { get; set; }
}
