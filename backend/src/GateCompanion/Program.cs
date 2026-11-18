using GateCompanion;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options => options.ServiceName = "ETicket Gate Companion");
builder.Services.Configure<CompanionOptions>(builder.Configuration.GetSection("GateCompanion"));
builder.Services.AddHttpClient("ETicketApi");
builder.Services.AddSingleton<IDeviceAdapter, SimulatedDeviceAdapter>();
builder.Services.AddHostedService<CompanionWorker>();
await builder.Build().RunAsync();
