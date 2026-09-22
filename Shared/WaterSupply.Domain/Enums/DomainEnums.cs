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
    NoSupply,
    Other
}

public enum RequestStatus
{
    Open,
    InProgress,
    Closed
}
