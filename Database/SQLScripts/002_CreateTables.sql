USE WaterSupplyManagement;
GO

IF OBJECT_ID(N'dbo.Residents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Residents
    (
        ResidentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Residents PRIMARY KEY,
        IdentityUserId NVARCHAR(450) NULL CONSTRAINT UQ_Residents_IdentityUserId UNIQUE,
        FullName NVARCHAR(120) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        Phone NVARCHAR(30) NOT NULL,
        Address NVARCHAR(300) NOT NULL,
        RegistrationDate DATE NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Residents_IsActive DEFAULT 1
    );
END
GO

IF OBJECT_ID(N'dbo.WaterConnections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WaterConnections
    (
        WaterConnectionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WaterConnections PRIMARY KEY,
        ResidentId INT NOT NULL,
        ConnectionNumber NVARCHAR(40) NOT NULL CONSTRAINT UQ_WaterConnections_ConnectionNumber UNIQUE,
        ConnectionType NVARCHAR(20) NOT NULL CONSTRAINT CK_WaterConnections_ConnectionType CHECK (ConnectionType IN (N'Residential', N'Commercial')),
        MeterNumber NVARCHAR(40) NOT NULL,
        ConnectionDate DATE NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT CK_WaterConnections_Status CHECK (Status IN (N'Active', N'Suspended', N'Closed')),
        CONSTRAINT FK_WaterConnections_Residents FOREIGN KEY (ResidentId) REFERENCES dbo.Residents(ResidentId)
    );
END
GO

IF OBJECT_ID(N'dbo.MeterReadings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MeterReadings
    (
        MeterReadingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MeterReadings PRIMARY KEY,
        WaterConnectionId INT NOT NULL,
        ReadingDate DATE NOT NULL,
        PreviousReading DECIMAL(18,2) NOT NULL CONSTRAINT CK_MeterReadings_PreviousReading CHECK (PreviousReading >= 0),
        CurrentReading DECIMAL(18,2) NOT NULL CONSTRAINT CK_MeterReadings_CurrentReading CHECK (CurrentReading >= PreviousReading),
        Consumption AS (CurrentReading - PreviousReading) PERSISTED,
        CONSTRAINT UQ_MeterReadings_ConnectionDate UNIQUE (WaterConnectionId, ReadingDate),
        CONSTRAINT FK_MeterReadings_WaterConnections FOREIGN KEY (WaterConnectionId) REFERENCES dbo.WaterConnections(WaterConnectionId)
    );
END
GO

IF OBJECT_ID(N'dbo.Bills', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bills
    (
        BillId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bills PRIMARY KEY,
        WaterConnectionId INT NOT NULL,
        BillingPeriodStart DATE NOT NULL,
        BillingPeriodEnd DATE NOT NULL,
        UnitsConsumed DECIMAL(18,2) NOT NULL CONSTRAINT CK_Bills_UnitsConsumed CHECK (UnitsConsumed >= 0),
        RatePerUnit DECIMAL(18,2) NOT NULL CONSTRAINT CK_Bills_RatePerUnit CHECK (RatePerUnit >= 0),
        FixedCharge DECIMAL(18,2) NOT NULL CONSTRAINT CK_Bills_FixedCharge CHECK (FixedCharge >= 0),
        TaxAmount DECIMAL(18,2) NOT NULL CONSTRAINT CK_Bills_TaxAmount CHECK (TaxAmount >= 0),
        TotalAmount AS (UnitsConsumed * RatePerUnit + FixedCharge + TaxAmount) PERSISTED,
        PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Bills_PaidAmount DEFAULT 0,
        DueDate DATE NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT CK_Bills_Status CHECK (Status IN (N'Unpaid', N'PartiallyPaid', N'Paid', N'Overdue')),
        CONSTRAINT UQ_Bills_ConnectionPeriod UNIQUE (WaterConnectionId, BillingPeriodStart, BillingPeriodEnd),
        CONSTRAINT FK_Bills_WaterConnections FOREIGN KEY (WaterConnectionId) REFERENCES dbo.WaterConnections(WaterConnectionId)
    );
END
GO

IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments
    (
        PaymentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
        BillId INT NOT NULL,
        PaymentDate DATETIME2 NOT NULL,
        Amount DECIMAL(18,2) NOT NULL CONSTRAINT CK_Payments_Amount CHECK (Amount > 0),
        PaymentMethod NVARCHAR(20) NOT NULL,
        ReferenceNumber NVARCHAR(60) NOT NULL CONSTRAINT UQ_Payments_ReferenceNumber UNIQUE,
        CONSTRAINT FK_Payments_Bills FOREIGN KEY (BillId) REFERENCES dbo.Bills(BillId)
    );
END
GO

IF OBJECT_ID(N'dbo.ServiceRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ServiceRequests
    (
        ServiceRequestId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ServiceRequests PRIMARY KEY,
        ResidentId INT NOT NULL,
        WaterConnectionId INT NULL,
        RequestType NVARCHAR(30) NOT NULL,
        Description NVARCHAR(1000) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        ResolvedAt DATETIME2 NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT CK_ServiceRequests_Status CHECK (Status IN (N'Open', N'InProgress', N'Resolved', N'Rejected')),
        StaffNotes NVARCHAR(1000) NULL,
        CONSTRAINT FK_ServiceRequests_Residents FOREIGN KEY (ResidentId) REFERENCES dbo.Residents(ResidentId),
        CONSTRAINT FK_ServiceRequests_WaterConnections FOREIGN KEY (WaterConnectionId) REFERENCES dbo.WaterConnections(WaterConnectionId)
    );
END
GO

IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications
    (
        NotificationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
        ResidentId INT NOT NULL,
        Title NVARCHAR(160) NOT NULL,
        Message NVARCHAR(1000) NOT NULL,
        NotificationType NVARCHAR(30) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        ReadAt DATETIME2 NULL,
        CONSTRAINT FK_Notifications_Residents FOREIGN KEY (ResidentId) REFERENCES dbo.Residents(ResidentId)
    );
END
GO
