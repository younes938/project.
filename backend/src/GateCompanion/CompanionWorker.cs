using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace GateCompanion;

public sealed class CompanionOptions
{
    public string ApiBaseUrl { get; set; } = "http://localhost:5009";
    public string GateId { get; set; } = "GATE-01";
    public int IntervalSeconds { get; set; } = 15;
    public bool SimulationMode { get; set; } = true;
}

public sealed record DeviceReading(
    string DeviceId,
    string DeviceType,
    string Status,
    DateTimeOffset Timestamp,
    double? Value = null,
    string? Unit = null,
    string? Message = null
);

public interface IDeviceAdapter
{
    Task<IReadOnlyList<DeviceReading>> ReadAsync(CancellationToken cancellationToken);
}

// Safe starter adapter. Replace with vendor SDK / TCP / serial implementations after hardware models are known.
public sealed class SimulatedDeviceAdapter : IDeviceAdapter
{
    public Task<IReadOnlyList<DeviceReading>> ReadAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var simulatedWeight = RandomNumberGenerator.GetInt32(8_000, 28_001);

        IReadOnlyList<DeviceReading> readings =
        [
            new("CAM-01", "anpr-camera", "simulated", now, Message: "Waiting for camera integration"),
            new("SCALE-01", "weighbridge", "simulated", now, simulatedWeight, "kg", "Simulated reading; not a real measurement"),
            new("RFID-01", "rfid-reader", "simulated", now, Message: "Waiting for reader integration"),
            new("BARRIER-01", "barrier", "simulated", now, Message: "No barrier command sent")
        ];

        return Task.FromResult(readings);
    }
}

public sealed class CompanionWorker(
    ILogger<CompanionWorker> logger,
    IHttpClientFactory clients,
    IDeviceAdapter devices,
    IOptions<CompanionOptions> options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        var client = clients.CreateClient("ETicketApi");

        client.BaseAddress = new Uri(settings.ApiBaseUrl.TrimEnd('/') + "/");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var readings = await devices.ReadAsync(stoppingToken);

                var payload = new
                {
                    gateId = settings.GateId,
                    agentId = Environment.MachineName,
                    mode = settings.SimulationMode ? "simulation" : "hardware",
                    timestamp = DateTimeOffset.UtcNow,
                    devices = readings
                };

                using var response = await client.PostAsJsonAsync(
                    "api/gate-companion/heartbeat",
                    payload,
                    stoppingToken
                );

                response.EnsureSuccessStatusCode();

                logger.LogInformation(
                    "Gate {GateId}: sent {Count} device statuses",
                    settings.GateId,
                    readings.Count
                );
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Could not report gate device status to {ApiUrl}",
                    settings.ApiBaseUrl
                );
            }

            await Task.Delay(
                TimeSpan.FromSeconds(Math.Clamp(settings.IntervalSeconds, 5, 300)),
                stoppingToken
            );
        }
    }
}