using System.ComponentModel.DataAnnotations;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models.ServiceRequestViewModels;

public sealed class ServiceRequestCreateViewModel
{
    [Range(1, int.MaxValue)]
    public int ResidentId { get; set; }

    public int? WaterConnectionId { get; set; }

    [Required]
    public ServiceRequestType RequestType { get; set; } = ServiceRequestType.Other;

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}

public sealed class ServiceRequestStatusViewModel
{
    [Required]
    public ServiceRequestStatus Status { get; set; }

    [StringLength(1000)]
    public string? StaffNotes { get; set; }
}
