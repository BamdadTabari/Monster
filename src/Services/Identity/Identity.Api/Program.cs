using Hellang.Middleware.ProblemDetails;
using Identity.Application;
using Identity.Application.Options;
using Identity.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Monster.BuildingBlocks;
using Serilog;
using System.Net;
using System.Text;

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
builder.Services.AddHttpContextAccessor();

// MediatR scanning
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IApplicationAssemblyMarker).Assembly));


builder.Services.AddMonsterBuildingBlocks();  // ProblemDetails + Validation pipeline + providers
builder.Services.AddIdentityApplication();
// Infra (Db + repo + uow + jwt + hasher + stores)
builder.Services.AddIdentityInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>("identity-db");

// Auth validation (same JwtOptions)
var jwtOpt = new JwtOptions();
builder.Configuration.GetSection("Jwt").Bind(jwtOpt);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOpt.Issuer,
            ValidAudience = jwtOpt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(5)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();


app.UseMonsterWebPipeline(); // Serilog request logging + ProblemDetails + Correlation-ID

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Health endpoints
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.UseCorrelationId();           
app.UseGlobalProblemDetails();    
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
