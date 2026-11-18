/* Run after 001_create_passengers.sql against ETicketPassengers. Safe to re-run. */
USE ETicketPassengers;
GO

IF OBJECT_ID(N'dbo.Shipments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Shipments
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Shipments PRIMARY KEY,
        ShipmentNumber NVARCHAR(60) NOT NULL,
        GoodsType NVARCHAR(120) NOT NULL,
        WeightKg DECIMAL(18,3) NOT NULL,
        OriginCountry NVARCHAR(80) NOT NULL,
        DestinationCountry NVARCHAR(80) NOT NULL,
        CustomsStatus NVARCHAR(24) NOT NULL,
        Notes NVARCHAR(1000) NOT NULL CONSTRAINT DF_Shipments_Notes DEFAULT N'',
        CreatedAt DATETIMEOFFSET NOT NULL,
        UpdatedAt DATETIMEOFFSET NOT NULL,
        CONSTRAINT CK_Shipments_Weight CHECK (WeightKg > 0),
        CONSTRAINT CK_Shipments_CustomsStatus CHECK (CustomsStatus IN (N'Approved', N'Denied', N'Under Inspection'))
    );
    CREATE UNIQUE INDEX UX_Shipments_ShipmentNumber ON dbo.Shipments(ShipmentNumber);
    CREATE INDEX IX_Shipments_CreatedAt ON dbo.Shipments(CreatedAt DESC);
END;
GO

IF OBJECT_ID(N'dbo.Inspections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inspections
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Inspections PRIMARY KEY,
        TargetType NVARCHAR(24) NOT NULL,
        TargetReference NVARCHAR(100) NOT NULL,
        Notes NVARCHAR(2000) NOT NULL,
        Status NVARCHAR(24) NOT NULL,
        SecurityAlert BIT NOT NULL,
        AlertDetails NVARCHAR(1000) NOT NULL CONSTRAINT DF_Inspections_AlertDetails DEFAULT N'',
        CreatedAt DATETIMEOFFSET NOT NULL,
        CreatedBy NVARCHAR(120) NOT NULL,
        CONSTRAINT CK_Inspections_TargetType CHECK (TargetType IN (N'Passenger', N'Vehicle', N'Shipment')),
        CONSTRAINT CK_Inspections_Status CHECK (Status IN (N'Clear', N'Suspicious', N'Requires Review'))
    );
    CREATE INDEX IX_Inspections_CreatedAt ON dbo.Inspections(CreatedAt DESC);
    CREATE INDEX IX_Inspections_SecurityAlert ON dbo.Inspections(SecurityAlert, CreatedAt DESC);
END;
GO

IF OBJECT_ID(N'dbo.AuditRecords', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditRecords
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditRecords PRIMARY KEY,
        EventType NVARCHAR(80) NOT NULL,
        UserId UNIQUEIDENTIFIER NULL,
        DeviceContext NVARCHAR(200) NOT NULL,
        [Timestamp] DATETIMEOFFSET NOT NULL,
        EntityType NVARCHAR(80) NOT NULL,
        EntityId NVARCHAR(100) NULL,
        [Action] NVARCHAR(80) NOT NULL,
        PreviousValueJson NVARCHAR(MAX) NULL,
        NewValueJson NVARCHAR(MAX) NULL,
        Result NVARCHAR(40) NOT NULL
    );
    CREATE INDEX IX_AuditRecords_Timestamp ON dbo.AuditRecords([Timestamp] DESC);
    CREATE INDEX IX_AuditRecords_UserId_Timestamp ON dbo.AuditRecords(UserId, [Timestamp] DESC);
END;
GO
