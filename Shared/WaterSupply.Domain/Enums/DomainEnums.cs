namespace WaterSupply.Domain.Enums;

public enum ConnectionType
{
    Residential,
    Commercial
}

public enum ConnectionStatus
{
    Active,
    Suspended,
    Closed
}

public enum BillStatus
{
    Unpaid,
    PartiallyPaid,
    Paid,
    Overdue
}

public enum PaymentMethod
{
    Cash,
    Online,
    BankTransfer
}

public enum ServiceRequestType
{
    Leakage,
    LowPressure,
    NewConnection,
    BillingQuery,
    Other
}

public enum ServiceRequestStatus
{
    Open,
    InProgress,
    Resolved,
    Rejected
}

public enum NotificationType
{
    BillReminder,
    ServiceRequestUpdate,
    System
}
