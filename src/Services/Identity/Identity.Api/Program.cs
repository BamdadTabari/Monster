using Hellang.Middleware.ProblemDetails;
using Identity.Application;
using Identity.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monster.BuildingBlocks;
using Serilog;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddMonsterBuildingBlocks();  // ProblemDetails + Validation pipeline + providers
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>("identity-db");

var app = builder.Build();


app.UseMonsterWebPipeline(); // Serilog request logging + ProblemDetails + Correlation-ID

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Health endpoints
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// Info + ping
app.MapGet("/_info", () => Results.Json(new
{
    service = "Identity.Api",
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
