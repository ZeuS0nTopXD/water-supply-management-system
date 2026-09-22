namespace WaterSupply.Web.Models.MeterReadingViewModels;

public sealed class MeterReadingListItemViewModel
{
    public string ConnectionNumber { get; init; } = string.Empty;
    public DateOnly ReadingDate { get; init; }
    public decimal PreviousReading { get; init; }
    public decimal CurrentReading { get; init; }
    public decimal Consumption { get; init; }
}
