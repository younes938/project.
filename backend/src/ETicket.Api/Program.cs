using ETicket.Api;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using ETicket.Api.ErrorHandling;
using ETicket.Api.OperatingContext;
using ETicket.Api.Passengers;
using ETicket.Api.Operations;
using ETicket.Api.Operations.Persistence;
using ETicket.Api.Passengers.Persistence;
using ETicket.Api.Security;
using ETicket.Api.VehicleEntry;
using ETicket.Api.VehicleEntry.Persistence;
using ETicket.Application.Identity;
using ETicket.Application.OperatingContexts;
using ETicket.Application.Ports;
using ETicket.Application.VehicleEntries;
using ETicket.Domain.Enums;
using ETicket.Domain.Services;
using ETicket.Infrastructure.Audit;
using ETicket.Infrastructure.Persistence;
using ETicket.Infrastructure.Time;
using ETicket.Integrations.Mock;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

const string sessionScheme = "ETicket.Session";

var builder = WebApplication.CreateBuilder(args);

var sessionOptions = new ETicketSessionOptions
{
    EnableMockSignIn = builder.Configuration.GetValue<bool>(
        $"{ETicketSessionOptions.SectionName}:EnableMockSignIn"),
    IdleTimeoutMinutes = builder.Configuration.GetValue<int?>(
        $"{ETicketSessionOptions.SectionName}:IdleTimeoutMinutes") ?? 15
};

if (sessionOptions.IdleTimeoutMinutes is < 1 or > 720)
{
    throw new InvalidOperationException(
        "ETicket authentication idle timeout must be between 1 and 720 minutes.");
}

builder.Services.AddSingleton(sessionOptions);
builder.Services.AddOpenApi();
builder.Services.AddGateCompanion();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = builder.Environment.IsDevelopment()
        ? "ETicket.Antiforgery"
        : "__Host-eticket-antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services
    .AddAuthentication(sessionScheme)
    .AddCookie(sessionScheme, options =>
    {
        options.Cookie.Name = builder.Environment.IsDevelopment()
            ? "ETicket.Session"
            : "__Host-eticket-session";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionOptions.IdleTimeoutMinutes);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            },
            OnValidatePrincipal = async context =>
            {
                var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdValue, out var userId))
                {
                    context.RejectPrincipal();
                    return;
                }

                var authAdapter = context.HttpContext.RequestServices.GetRequiredService<IAuthAdapter>();
                var user = await authAdapter.GetUserAsync(userId, context.HttpContext.RequestAborted);
                if (user is not { IsActive: true })
                {
                    context.RejectPrincipal();
                    return;
                }

                var methodClaim = context.Principal?.FindFirstValue(
                    ETicketClaimTypes.AuthenticationMethod);
                if (!Enum.TryParse<AuthenticationMethod>(methodClaim, true, out var method))
                {
                    context.RejectPrincipal();
                    return;
                }

                context.ReplacePrincipal(UserPrincipalFactory.Create(user, method));
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Enum.GetValues<Permission>())
    {
        options.AddPolicy(
            PermissionPolicies.For(permission),
            policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(
                    ETicketClaimTypes.Permission,
                    PermissionClaims.FromPermission(permission)));
    }
});

var passengerStorage = builder.Configuration["ETicket:PassengerStorage"] ?? "Mock";
if (string.Equals(passengerStorage, "SqlServer", StringComparison.OrdinalIgnoreCase))
{
    var passengerConnectionString = builder.Configuration.GetConnectionString("Passengers");
    if (string.IsNullOrWhiteSpace(passengerConnectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:Passengers is required when ETicket:PassengerStorage is SqlServer.");
    }

    builder.Services.AddDbContext<PassengerDbContext>(options =>
        options.UseSqlServer(passengerConnectionString, sql => sql.EnableRetryOnFailure()));
    builder.Services.AddScoped<IPassengerRegistry, SqlServerPassengerRegistry>();
}
else if (string.Equals(passengerStorage, "Mock", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<InMemoryPassengerRegistry>();
    builder.Services.AddSingleton<IPassengerRegistry>(services =>
        services.GetRequiredService<InMemoryPassengerRegistry>());
}
else
{
    throw new InvalidOperationException(
        "ETicket:PassengerStorage must be either Mock or SqlServer.");
}

builder.Services.AddSingleton<VehicleStateTransitionService>();
builder.Services.AddSingleton<AccessControlService>();
builder.Services.AddSingleton<OperatingContextSelectionService>();
builder.Services.AddSingleton<VehicleEntryValidationService>();
builder.Services.AddScoped<OperatingContextService>();
builder.Services.AddScoped<VehicleEntryService>();
builder.Services.AddSingleton<IClock, ETicket.Infrastructure.Time.SystemClock>();
var vehicleVisitStorage = builder.Configuration["ETicket:VehicleVisitStorage"] ?? "SqlServer";
if (string.Equals(vehicleVisitStorage, "SqlServer", StringComparison.OrdinalIgnoreCase))
{
    var vehicleConnectionString = builder.Configuration.GetConnectionString("Passengers");
    if (string.IsNullOrWhiteSpace(vehicleConnectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:Passengers is required when ETicket:VehicleVisitStorage is SqlServer.");
    }

    builder.Services.AddDbContext<VehicleVisitDbContext>(options =>
        options.UseSqlServer(vehicleConnectionString, sql => sql.EnableRetryOnFailure()));
    builder.Services.AddScoped<IVehicleVisitRepository, SqlServerVehicleVisitRepository>();
}
else if (string.Equals(vehicleVisitStorage, "Mock", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IVehicleVisitRepository, InMemoryVehicleVisitRepository>();
}
else
{
    throw new InvalidOperationException("ETicket:VehicleVisitStorage must be either Mock or SqlServer.");
}
builder.Services.AddSingleton<ICheckpointCatalog, InMemoryCheckpointCatalog>();
builder.Services.AddSingleton<IOperatingContextRepository, InMemoryOperatingContextRepository>();
builder.Services.AddDbContext<OperationsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Passengers"), sql => sql.EnableRetryOnFailure()));
builder.Services.AddScoped<IAuditSink, SqlServerAuditSink>();

var adapterMode = builder.Configuration["ETicket:AdapterMode"] ?? "Mock";
if (!string.Equals(adapterMode, "Mock", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Only the Mock adapter mode is available during Phase 4.");
}

builder.Services.AddSingleton<IAuthAdapter, MockAuthAdapter>();
builder.Services.AddSingleton<IVmsAdapter, MockVmsAdapter>();
builder.Services.AddSingleton<IEQueueAdapter, MockEQueueAdapter>();
builder.Services.AddSingleton<ICustomsAdapter, MockCustomsAdapter>();
builder.Services.AddSingleton<IBorderControlAdapter, MockBorderControlAdapter>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health");

app.MapGet(
        "/api/system/info",
        () => Results.Ok(new
        {
            service = "ETicket.Api",
            phase = "vehicle-entry",
            adapterMode
        }))
    .WithName("SystemInfo");

app.MapGet(
        "/api/auth/antiforgery",
        (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            context.Response.Cookies.Append(
                "XSRF-TOKEN",
                tokens.RequestToken
                    ?? throw new InvalidOperationException("Antiforgery request token was not created."),
                new CookieOptions
                {
                    HttpOnly = false,
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    Secure = !app.Environment.IsDevelopment() || context.Request.IsHttps,
                    Path = "/"
                });
            context.Response.Headers.CacheControl = "no-store";
            return Results.NoContent();
        })
    .AllowAnonymous()
    .WithName("IssueAntiforgeryToken");

if (app.Environment.IsDevelopment() && sessionOptions.EnableMockSignIn)
{
    app.MapPost(
            "/api/auth/mock/sign-in",
            async (
                HttpContext context,
                IAuthAdapter authAdapter,
                IAuditSink auditSink,
                IClock clock,
                CancellationToken cancellationToken) =>
            {
                var user = await authAdapter.AuthenticateAsync(
                    new AuthenticationRequest(
                        "phase2-demo-operator",
                        AuthenticationMethod.Mock),
                    cancellationToken);

                if (user is not { IsActive: true })
                {
                    await AuthAudit.WriteAsync(
                        auditSink,
                        clock,
                        context,
                        "SignIn",
                        "Denied",
                        user?.Id,
                        cancellationToken);
                    return Results.Unauthorized();
                }

                var principal = UserPrincipalFactory.Create(user, AuthenticationMethod.Mock);
                await context.SignInAsync(
                    sessionScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        IsPersistent = false
                    });
                await AuthAudit.WriteAsync(
                    auditSink,
                    clock,
                    context,
                    "SignIn",
                    "Success",
                    user.Id,
                    cancellationToken);

                return Results.Ok(
                    SessionResponse.FromPrincipal(
                        principal,
                        sessionOptions.IdleTimeoutMinutes));
            })
        .AllowAnonymous()
        .AddEndpointFilter<AntiforgeryValidationFilter>()
        .WithName("MockSignIn");
}

app.MapGet(
        "/api/auth/session",
        (ClaimsPrincipal principal) =>
            Results.Ok(
                SessionResponse.FromPrincipal(
                    principal,
                    sessionOptions.IdleTimeoutMinutes)))
    .RequireAuthorization()
    .WithName("GetSession");

app.MapPost(
        "/api/auth/activity",
        () => Results.NoContent())
    .RequireAuthorization()
    .AddEndpointFilter<AntiforgeryValidationFilter>()
    .WithName("RecordSessionActivity");

app.MapPost(
        "/api/auth/logout",
        async (
            HttpContext context,
            IAuditSink auditSink,
            IClock clock,
            CancellationToken cancellationToken) =>
        {
            await AuthAudit.WriteAsync(
                auditSink,
                clock,
                context,
                "SignOut",
                "Success",
                null,
                cancellationToken);
            await context.SignOutAsync(sessionScheme);
            return Results.NoContent();
        })
    .RequireAuthorization()
    .AddEndpointFilter<AntiforgeryValidationFilter>()
    .WithName("SignOut");

app.MapGet(
        "/api/auth/permissions/vehicle-view",
        () => Results.NoContent())
    .RequireAuthorization(PermissionPolicies.For(Permission.ViewVehicle))
    .WithName("CheckVehicleViewPermission");

app.MapGet(
        "/api/auth/checkpoints/{checkpointId:guid}/access",
        (Guid checkpointId, ClaimsPrincipal principal) =>
            principal.CanAccessCheckpoint(checkpointId)
                ? Results.NoContent()
                : Results.Forbid())
    .RequireAuthorization()
    .WithName("CheckCheckpointAccess");

app.MapOperatingContextEndpoints();
app.MapVehicleEntryEndpoints();
app.MapVehicleVisitEndpoints();
app.MapPassengerEndpoints();
app.MapOperationsEndpoints();
app.MapGateCompanionEndpoints();

app.Run();

public partial class Program;
