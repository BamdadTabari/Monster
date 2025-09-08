using Content.Application;
using Content.Infrastructure;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monster.BuildingBlocks;
using Monster.BuildingBlocks.Messaging;
using Monster.BuildingBlocks.Outbox;
using Serilog;
using System.Net;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, services, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console();
});

// ---- Services ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// BuildingBlocks (ProblemDetails, Validation pipeline, providers, etc.)
builder.Services.AddMonsterBuildingBlocks();

// Content layers
builder.Services.AddContentApplication();
builder.Services.AddContentInfrastructure(builder.Configuration);

// Health checks (DB)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ContentDbContext>("content-db");

// ---- App ----
var app = builder.Build();

app.UseMonsterWebPipeline(); // Serilog request logging + ProblemDetails + Correlation-ID

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health endpoints
app.MapHealthChecks("/health/live");  // basic liveness
app.MapHealthChecks("/health/ready"); // readiness (same for now; can extend with custom predicate)

app.UseCorrelationId();     
app.UseSerilogRequestLogging(o =>
{
    o.MessageTemplate = "HTTP {RequestMethod} {RequestPath} → {StatusCode} in {Elapsed:0.0000} ms";
});      
app.UseGlobalProblemDetails();    
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
