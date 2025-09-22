using Content.Application;
using Content.Infrastructure;
using Monster.BuildingBlocks.Logging;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// serilog
builder.ConfigureSerilog();

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

app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} → {StatusCode} in {Elapsed:0.0000} ms";
    opts.EnrichDiagnosticContext = (diag, http) =>
    {
        // CorrelationId from your middleware (safe fallback)
        var cid = http.Response.Headers["X-Correlation-ID"].ToString();
        diag.Set("CorrelationId", string.IsNullOrWhiteSpace(cid) ? "n/a" : cid);

        // Client IP (X-Forwarded-For first, else RemoteIpAddress)
        string clientIp = "unknown";
        if (http.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd) && fwd.Count > 0)
        {
            clientIp = fwd.ToString().Split(',')[0].Trim();
        }
        else
        {
            clientIp = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
        diag.Set("ClientIP", clientIp);

        // Authenticated user (optional)
        var user = http.User?.Identity?.IsAuthenticated == true ? (http.User.Identity?.Name ?? "authenticated") : "anonymous";
        diag.Set("User", user);
    };
});    
app.UseGlobalProblemDetails();    
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
