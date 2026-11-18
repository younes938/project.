/*
  Normalize existing shipment and inspection values to the English values
  used by the Angular forms and ASP.NET Core validation.
  Run once against ETicketPassengers. Safe to re-run.
*/
USE ETicketPassengers;
GO

IF OBJECT_ID(N'dbo.Shipments', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Shipments_CustomsStatus' AND parent_object_id = OBJECT_ID(N'dbo.Shipments'))
        ALTER TABLE dbo.Shipments DROP CONSTRAINT CK_Shipments_CustomsStatus;

    UPDATE dbo.Shipments
       SET CustomsStatus = CASE CustomsStatus
           WHEN N'موافق' THEN N'Approved'
           WHEN N'مرفوض' THEN N'Denied'
           WHEN N'قيد التفتيش' THEN N'Under Inspection'
           ELSE CustomsStatus END;

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Shipments_CustomsStatus' AND parent_object_id = OBJECT_ID(N'dbo.Shipments'))
        ALTER TABLE dbo.Shipments ADD CONSTRAINT CK_Shipments_CustomsStatus
            CHECK (CustomsStatus IN (N'Approved', N'Denied', N'Under Inspection'));
END;
GO

IF OBJECT_ID(N'dbo.Inspections', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Inspections_TargetType' AND parent_object_id = OBJECT_ID(N'dbo.Inspections'))
        ALTER TABLE dbo.Inspections DROP CONSTRAINT CK_Inspections_TargetType;
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Inspections_Status' AND parent_object_id = OBJECT_ID(N'dbo.Inspections'))
        ALTER TABLE dbo.Inspections DROP CONSTRAINT CK_Inspections_Status;

    UPDATE dbo.Inspections
       SET TargetType = CASE TargetType
           WHEN N'مسافر' THEN N'Passenger'
           WHEN N'مركبة' THEN N'Vehicle'
           WHEN N'شحنة' THEN N'Shipment'
           ELSE TargetType END,
           Status = CASE Status
           WHEN N'سليم' THEN N'Clear'
           WHEN N'مشبوه' THEN N'Suspicious'
           WHEN N'يحتاج مراجعة' THEN N'Requires Review'
           ELSE Status END;

    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Inspections_TargetType' AND parent_object_id = OBJECT_ID(N'dbo.Inspections'))
        ALTER TABLE dbo.Inspections ADD CONSTRAINT CK_Inspections_TargetType
            CHECK (TargetType IN (N'Passenger', N'Vehicle', N'Shipment'));
    IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Inspections_Status' AND parent_object_id = OBJECT_ID(N'dbo.Inspections'))
        ALTER TABLE dbo.Inspections ADD CONSTRAINT CK_Inspections_Status
            CHECK (Status IN (N'Clear', N'Suspicious', N'Requires Review'));
END;
GO
