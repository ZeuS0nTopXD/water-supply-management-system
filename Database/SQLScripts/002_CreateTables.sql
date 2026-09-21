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
        Status NVARCHAR(20) NOT NULL CONSTRAINT CK_WaterConnections_Status CHECK (Status IN (N'Active', N'Inactive')),
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
        Consumption DECIMAL(18,2) NOT NULL,
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
        MeterReadingId INT NOT NULL,
        BillDate DATE NOT NULL,
        UnitsConsumed INT NOT NULL CONSTRAINT CK_Bills_UnitsConsumed CHECK (UnitsConsumed >= 0),
        RatePerUnit DECIMAL(18,2) NOT NULL CONSTRAINT CK_Bills_RatePerUnit CHECK (RatePerUnit >= 0),
        TotalAmount DECIMAL(18,2) NOT NULL CONSTRAINT CK_Bills_TotalAmount CHECK (TotalAmount >= 0),
        DueDate DATE NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT CK_Bills_Status CHECK (Status IN (N'Unpaid', N'Paid')),
        CONSTRAINT UQ_Bills_ConnectionDate UNIQUE (WaterConnectionId, BillDate),
        CONSTRAINT FK_Bills_WaterConnections FOREIGN KEY (WaterConnectionId) REFERENCES dbo.WaterConnections(WaterConnectionId),
        CONSTRAINT FK_Bills_MeterReadings FOREIGN KEY (MeterReadingId) REFERENCES dbo.MeterReadings(MeterReadingId)
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
        RequestType NVARCHAR(20) NOT NULL CONSTRAINT CK_ServiceRequests_RequestType CHECK (RequestType IN (N'Leak', N'NoSupply', N'Other')),
        Description NVARCHAR(1000) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT CK_ServiceRequests_Status CHECK (Status IN (N'Open', N'InProgress', N'Closed')),
        StaffNotes NVARCHAR(1000) NULL,
        CONSTRAINT FK_ServiceRequests_Residents FOREIGN KEY (ResidentId) REFERENCES dbo.Residents(ResidentId),
        CONSTRAINT FK_ServiceRequests_WaterConnections FOREIGN KEY (WaterConnectionId) REFERENCES dbo.WaterConnections(WaterConnectionId)
    );
END
GO
