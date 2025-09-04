using Content.BackgroundWorkers;
using Content.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Monster.BuildingBlocks;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Services.AddLogging(lb => lb.AddSerilog());

// Config + Db
builder.Services.AddContentInfrastructure(builder.Configuration);

// Background workers
builder.Services.AddHostedService<OutboxPublisher>();

var host = builder.Build();
await host.RunAsync();

public partial class Program { }
