using System.ComponentModel.DataAnnotations;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Models.ServiceRequestViewModels;

public sealed class ServiceRequestCreateViewModel
{
    [Display(Name = "Resident")]
    [Range(1, int.MaxValue)]
    public int ResidentId { get; set; }

    [Display(Name = "Water connection")]
    public int? WaterConnectionId { get; set; }

    [Display(Name = "Request type")]
    [Required]
    public RequestType RequestType { get; set; } = RequestType.Other;

    [Display(Name = "Description")]
    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}

public sealed class ServiceRequestStatusViewModel
{
    [Display(Name = "Request status")]
    [Required]
    public RequestStatus Status { get; set; }

    [Display(Name = "Staff notes")]
    [StringLength(1000)]
    public string? StaffNotes { get; set; }
}
