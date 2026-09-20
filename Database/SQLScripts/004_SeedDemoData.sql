USE WaterSupplyManagement;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Residents WHERE ResidentId = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Residents ON;
    INSERT INTO dbo.Residents (ResidentId, FullName, Email, Phone, Address, RegistrationDate, IsActive)
    VALUES (1, N'Asha Patil', N'asha@example.com', N'9876543210', N'Main Road', '2026-01-01', 1),
           (2, N'Ravi Shah', N'ravi@example.com', N'9876543211', N'Lake Road', '2026-01-01', 1);
    SET IDENTITY_INSERT dbo.Residents OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.WaterConnections WHERE WaterConnectionId = 1)
BEGIN
    SET IDENTITY_INSERT dbo.WaterConnections ON;
    INSERT INTO dbo.WaterConnections (WaterConnectionId, ResidentId, ConnectionNumber, ConnectionType, MeterNumber, ConnectionDate, Status)
    VALUES (1, 1, N'WS-010', N'Residential', N'M-010', '2026-01-05', N'Active'),
           (2, 2, N'WS-020', N'Residential', N'M-020', '2026-01-05', N'Active');
    SET IDENTITY_INSERT dbo.WaterConnections OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MeterReadings WHERE MeterReadingId = 1)
BEGIN
    SET IDENTITY_INSERT dbo.MeterReadings ON;
    INSERT INTO dbo.MeterReadings (MeterReadingId, WaterConnectionId, ReadingDate, PreviousReading, CurrentReading)
    VALUES (1, 1, '2026-09-01', 100, 120), (2, 2, '2026-09-01', 200, 225);
    SET IDENTITY_INSERT dbo.MeterReadings OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Bills WHERE BillId = 1)
BEGIN
    SET IDENTITY_INSERT dbo.Bills ON;
    INSERT INTO dbo.Bills (BillId, WaterConnectionId, BillingPeriodStart, BillingPeriodEnd, UnitsConsumed, RatePerUnit, FixedCharge, TaxAmount, PaidAmount, DueDate, Status)
    VALUES (1, 1, '2026-09-01', '2026-09-30', 20, 5, 10, 2, 0, '2026-10-15', N'Unpaid'),
           (2, 2, '2026-09-01', '2026-09-30', 25, 5, 10, 2, 0, '2026-10-15', N'Unpaid');
    SET IDENTITY_INSERT dbo.Bills OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ServiceRequests WHERE ServiceRequestId = 1)
BEGIN
    SET IDENTITY_INSERT dbo.ServiceRequests ON;
    INSERT INTO dbo.ServiceRequests (ServiceRequestId, ResidentId, WaterConnectionId, RequestType, Description, CreatedAt, Status)
    VALUES (1, 1, 1, N'Leakage', N'Leakage reported near the meter.', '2026-09-10T09:00:00', N'Open');
    SET IDENTITY_INSERT dbo.ServiceRequests OFF;
END
GO
