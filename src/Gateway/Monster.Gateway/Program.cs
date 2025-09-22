using Monster.BuildingBlocks;
using Monster.BuildingBlocks.Logging;

var builder = WebApplication.CreateBuilder(args);
// serilog
builder.ConfigureSerilog();

// YARP + extras
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowDev", p => p
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()
        .SetIsOriginAllowed(_ => true)); // dev only; tighten later
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors("AllowDev");
app.UseMonsterWebPipeline();    // Serilog req log + ProblemDetails + Correlation-ID

app.MapHealthChecks("/health");
app.MapGet("/_info", () => Results.Json(new {
    service = "Monster.Gateway",
    env = app.Environment.EnvironmentName
}));

app.MapReverseProxy();          // <— the gateway

app.Run();
