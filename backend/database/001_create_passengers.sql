/*
  Run this script in SQL Server Management Studio against the ETicketPassengers database.
  It is safe to re-run: the table and indexes are created only when absent.
*/
IF DB_ID(N'ETicketPassengers') IS NULL
BEGIN
    CREATE DATABASE ETicketPassengers;
END;
GO

USE ETicketPassengers;
GO

IF OBJECT_ID(N'dbo.Passengers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Passengers
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Passengers PRIMARY KEY,
        FullName NVARCHAR(120) NOT NULL,
        PassportNumber NVARCHAR(40) NOT NULL,
        Nationality NVARCHAR(80) NOT NULL,
        OriginCountry NVARCHAR(80) NOT NULL,
        DestinationCountry NVARCHAR(80) NOT NULL,
        Status NVARCHAR(24) NOT NULL,
        Direction NVARCHAR(12) NOT NULL,
        Notes NVARCHAR(500) NOT NULL CONSTRAINT DF_Passengers_Notes DEFAULT N'',
        CreatedAt DATETIMEOFFSET NOT NULL,
        CONSTRAINT CK_Passengers_Status CHECK (Status IN (N'Allowed', N'Denied', N'Pending Verification')),
        CONSTRAINT CK_Passengers_Direction CHECK (Direction IN (N'Entry', N'Exit'))
    );

    CREATE INDEX IX_Passengers_PassportNumber ON dbo.Passengers(PassportNumber);
    CREATE INDEX IX_Passengers_CreatedAt ON dbo.Passengers(CreatedAt DESC);
END;
GO


IF OBJECT_ID(N'dbo.VehicleVisits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VehicleVisits
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VehicleVisits PRIMARY KEY,
        VehicleId UNIQUEIDENTIFIER NOT NULL,
        CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
        CheckpointId UNIQUEIDENTIFIER NOT NULL,
        EnteredAt DATETIMEOFFSET NOT NULL,
        Version BIGINT NOT NULL,
        IdempotencyKey NVARCHAR(100) NOT NULL,
        RequestFingerprint NVARCHAR(64) NOT NULL,
        VehicleJson NVARCHAR(MAX) NOT NULL,
        VisitJson NVARCHAR(MAX) NOT NULL,
        QueueEntryJson NVARCHAR(MAX) NULL
    );

    CREATE UNIQUE INDEX UX_VehicleVisits_CreatedByUserId_IdempotencyKey
        ON dbo.VehicleVisits(CreatedByUserId, IdempotencyKey);
    CREATE INDEX IX_VehicleVisits_CheckpointId_EnteredAt
        ON dbo.VehicleVisits(CheckpointId, EnteredAt DESC);
END;
GO
