namespace ETicket.Domain.Enums;

public enum VehicleVisitStatus
{
    EntryDenied,
    AwaitingControls,
    ControlFailed,
    AwaitingExit,
    Passed,
    Return,
    Cancelled
}

public enum ControlKind
{
    VehicleInspection,
    Border,
    Customs
}

public enum ControlStatus
{
    Pending,
    Passed,
    Failed,
    NotRequired,
    Cancelled
}

public enum GateMode
{
    RearExit,
    BorderExit,
    BorderEntry,
    RearEntry
}

public enum MovementDirection
{
    Entry,
    Exit
}

public enum VehicleEntryDecision
{
    Unspecified,
    Allow,
    Deny
}

public enum InspectionRejectionReason
{
    ProhibitedItems,
    DriverRequest,
    RegistrationMismatch,
    Other
}

public enum Permission
{
    ProcessVehicle,
    InspectVehicle,
    SearchRecentVehicles,
    SearchAllVehicles,
    ViewVehicle,
    EditVehicle,
    CancelVehicle,
    ExportVehicles,
    ViewQueue,
    ViewQueueHistory,
    PauseQueue,
    ResumeQueue,
    ViewStatistics,
    AdjustPersonsCount
}

public enum AuthenticationMethod
{
    Mock,
    Kep
}
