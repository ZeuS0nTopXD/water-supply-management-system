using System.ComponentModel.DataAnnotations;

namespace WaterSupply.Domain.Enums;

public enum ConnectionType
{
    Residential,
    Commercial
}

public enum ConnectionStatus
{
    Active,
    Inactive
}

public enum BillStatus
{
    Unpaid,
    Paid
}

public enum RequestType
{
    Leak,
    [Display(Name = "No supply")]
    NoSupply,
    Other
}

public enum RequestStatus
{
    Open,
    [Display(Name = "In progress")]
    InProgress,
    Closed
}
