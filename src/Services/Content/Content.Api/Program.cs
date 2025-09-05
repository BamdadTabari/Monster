using Content.Api.Endpoints;
using Content.Application;
using Content.Infrastructure;
using Hellang.Middleware.ProblemDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monster.BuildingBlocks;
using Serilog;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ---- Serilog (console) ----
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

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

app.MapControllers(); 

app.MapCategoryEndpoints();


// Health endpoints
app.MapHealthChecks("/health/live");  // basic liveness
app.MapHealthChecks("/health/ready"); // readiness (same for now; can extend with custom predicate)

// Minimal info & ping endpoints (for smoke tests)
app.MapGet("/_info", () => Results.Json(new
{
    service = "Content.Api",
    version = typeof(Program).Assembly.GetName().Version?.ToString(),
    environment = app.Environment.EnvironmentName
}, statusCode: StatusCodes.Status200OK));

app.MapGet("/api/ping", () =>
{
    var res = ResponseDto<string>.Ok("pong");
    return Results.Json(res, statusCode: (int)res.response_code);
})
.Produces((int)HttpStatusCode.OK);

app.Run();

public partial class Program { }
