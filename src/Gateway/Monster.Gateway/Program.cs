using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Monster.BuildingBlocks;
using Monster.BuildingBlocks.Logging;
using Monster.BuildingBlocks.Swagger;

var builder = WebApplication.CreateBuilder(args);
// serilog
builder.ConfigureSerilog();

builder.Services.AddSwaggerWithVersioning();

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

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUI(provider);
}
app.UseAuthorization();
app.MapControllers();

app.UseCors("AllowDev");
app.UseMonsterWebPipeline();    // Serilog req log + ProblemDetails + Correlation-ID

app.MapHealthChecks("/health");
app.MapGet("/_info", () => Results.Json(new {
    service = "Monster.Gateway",
    env = app.Environment.EnvironmentName
}));

app.MapReverseProxy();          // <— the gateway

app.Run();
