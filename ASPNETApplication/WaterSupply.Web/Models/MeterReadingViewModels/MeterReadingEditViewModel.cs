using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Web.Models.MeterReadingViewModels;

public sealed class MeterReadingEditViewModel
{
    [Range(1, int.MaxValue)]
    public int WaterConnectionId { get; set; }

    [Required]
    public DateOnly ReadingDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Range(0, double.MaxValue)]
    public decimal PreviousReading { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CurrentReading { get; set; }
}
