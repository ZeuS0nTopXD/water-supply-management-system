USE WaterSupplyManagement;
GO

IF OBJECT_ID(N'dbo.WaterConnectionRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WaterConnectionRequests
    (
        WaterConnectionRequestId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WaterConnectionRequests PRIMARY KEY,
        ResidentId INT NOT NULL,
        RequestedType NVARCHAR(20) NOT NULL CONSTRAINT CK_WaterConnectionRequests_RequestedType CHECK (RequestedType IN (N'Residential', N'Commercial')),
        ServiceAddress NVARCHAR(300) NOT NULL,
        Notes NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT CK_WaterConnectionRequests_Status CHECK (Status IN (N'Pending', N'InReview', N'Approved', N'Rejected')),
        StaffNotes NVARCHAR(1000) NULL,
        CONSTRAINT FK_WaterConnectionRequests_Residents FOREIGN KEY (ResidentId) REFERENCES dbo.Residents(ResidentId)
    );
END
GO
