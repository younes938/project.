/* Run against ETicketPassengers after the existing database scripts. Safe to re-run. */
USE ETicketPassengers;
GO

IF OBJECT_ID(N'dbo.WeightHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WeightHistory
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WeightHistory PRIMARY KEY,
        GateId NVARCHAR(80) NOT NULL,
        DeviceId NVARCHAR(80) NOT NULL,
        WeightKg DECIMAL(18,3) NOT NULL,
        WeightStatus NVARCHAR(20) NOT NULL,
        [Timestamp] DATETIMEOFFSET NOT NULL,
        CONSTRAINT CK_WeightHistory_Weight CHECK (WeightKg > 0),
        CONSTRAINT CK_WeightHistory_Status CHECK (WeightStatus IN (N'NORMAL', N'WARNING', N'OVERWEIGHT'))
    );
    CREATE INDEX IX_WeightHistory_Timestamp ON dbo.WeightHistory([Timestamp] DESC);
    CREATE INDEX IX_WeightHistory_GateId_Timestamp ON dbo.WeightHistory(GateId, [Timestamp] DESC);
END;
GO
