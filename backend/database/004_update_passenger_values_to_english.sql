/*
  Run against ETicketPassengers when dbo.Passengers already exists.
  Aligns the database constraints with the English values used by the Angular UI/API.
  Existing rows with Arabic status/direction values are converted before constraints are recreated.
*/
USE ETicketPassengers;
GO

IF OBJECT_ID(N'dbo.Passengers', N'U') IS NULL
    THROW 50001, 'dbo.Passengers does not exist. Run 001_create_passengers.sql first.', 1;
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Passengers_Status' AND parent_object_id = OBJECT_ID(N'dbo.Passengers'))
    ALTER TABLE dbo.Passengers DROP CONSTRAINT CK_Passengers_Status;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Passengers_Direction' AND parent_object_id = OBJECT_ID(N'dbo.Passengers'))
    ALTER TABLE dbo.Passengers DROP CONSTRAINT CK_Passengers_Direction;
GO

UPDATE dbo.Passengers
SET Status = CASE Status
    WHEN N'مسموح' THEN N'Allowed'
    WHEN N'مرفوض' THEN N'Denied'
    WHEN N'قيد التحقق' THEN N'Pending Verification'
    ELSE Status END,
    Direction = CASE Direction
    WHEN N'دخول' THEN N'Entry'
    WHEN N'خروج' THEN N'Exit'
    ELSE Direction END;
GO

ALTER TABLE dbo.Passengers WITH CHECK
ADD CONSTRAINT CK_Passengers_Status
CHECK (Status IN (N'Allowed', N'Denied', N'Pending Verification'));
ALTER TABLE dbo.Passengers WITH CHECK
ADD CONSTRAINT CK_Passengers_Direction
CHECK (Direction IN (N'Entry', N'Exit'));
GO
