using ETicket.Domain.Enums;

namespace ETicket.Api.Security;

public static class ETicketClaimTypes
{
    public const string AuthenticationMethod = "eticket:authentication_method";
    public const string Permission = "eticket:permission";
    public const string Checkpoint = "eticket:checkpoint";
}

public static class PermissionClaims
{
    public static string FromPermission(Permission permission) => permission switch
    {
        Permission.ProcessVehicle => "vehicle.process",
        Permission.InspectVehicle => "vehicle.inspect",
        Permission.SearchRecentVehicles => "vehicle.search.recent",
        Permission.SearchAllVehicles => "vehicle.search.all",
        Permission.ViewVehicle => "vehicle.view",
        Permission.EditVehicle => "vehicle.edit",
        Permission.CancelVehicle => "vehicle.cancel",
        Permission.ExportVehicles => "vehicle.export",
        Permission.ViewQueue => "queue.view",
        Permission.ViewQueueHistory => "queue.history.view",
        Permission.PauseQueue => "queue.pause",
        Permission.ResumeQueue => "queue.resume",
        Permission.ViewStatistics => "statistics.view",
        Permission.AdjustPersonsCount => "persons.adjust",
        _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null)
    };
}

public static class PermissionPolicies
{
    public static string For(Permission permission) => $"permission:{PermissionClaims.FromPermission(permission)}";
}
