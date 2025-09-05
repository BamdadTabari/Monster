using Identity.BackgroundWorkers;
using Identity.Infrastructure;
using Monster.BuildingBlocks;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Services.AddLogging(lb => lb.AddSerilog());

builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddHostedService<OutboxPublisher>();

var host = builder.Build();
await host.RunAsync();

public partial class Program { }
