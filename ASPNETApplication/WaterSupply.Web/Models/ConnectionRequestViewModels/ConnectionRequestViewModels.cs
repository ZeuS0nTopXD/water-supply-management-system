using System.ComponentModel.DataAnnotations;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models.ConnectionRequestViewModels;

public sealed class ConnectionRequestCreateViewModel
{
    [Display(Name = "Requested connection type")]
    [Required, EnumDataType(typeof(ConnectionType))]
    public ConnectionType RequestedType { get; set; } = ConnectionType.Residential;

    [Display(Name = "Service address")]
    [Required, StringLength(300, MinimumLength = 5)]
    public string ServiceAddress { get; set; } = string.Empty;

    [Display(Name = "Additional details")]
    [StringLength(1000)]
    public string? Notes { get; set; }
}

public sealed class ConnectionRequestStatusViewModel
{
    [Display(Name = "Request status")]
    [Required]
    public ConnectionRequestStatus Status { get; set; }

    [Display(Name = "Staff notes")]
    [StringLength(1000)]
    public string? StaffNotes { get; set; }
}

public sealed class ConnectionRequestListItemViewModel
{
    public int WaterConnectionRequestId { get; init; }
    public int ResidentId { get; init; }
    public string ResidentName { get; init; } = string.Empty;
    public string ServiceAddress { get; init; } = string.Empty;
    public ConnectionType RequestedType { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public ConnectionRequestStatus Status { get; init; }
    public string? StaffNotes { get; init; }

    public string StatusLabel => Status switch
    {
        ConnectionRequestStatus.InReview => "In review",
        ConnectionRequestStatus.Approved => "Approved",
        ConnectionRequestStatus.Rejected => "Rejected",
        _ => "Pending"
    };
}
