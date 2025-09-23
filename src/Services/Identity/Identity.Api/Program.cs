using Identity.Application;
using Identity.Application.Options;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.IdentityModel.Tokens;
using Monster.BuildingBlocks;
using Monster.BuildingBlocks.Logging;
using Monster.BuildingBlocks.Swagger;
using Serilog;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerWithVersioning();

// serilog
builder.ConfigureSerilog();

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

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
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear(); // trust all if you’re managing at the edge (or configure explicitly)
    o.KnownProxies.Clear();
});
var app = builder.Build();


app.UseMonsterWebPipeline(); // Serilog request logging + ProblemDetails + Correlation-ID

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUI(provider);
}


// Health endpoints
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

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
