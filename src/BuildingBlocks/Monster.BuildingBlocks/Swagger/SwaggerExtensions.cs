using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Monster.BuildingBlocks.Swagger
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerWithVersioning(this IServiceCollection services)
        {
            // 🔹 API Versioning
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // v1, v2
                options.SubstituteApiVersionInUrl = true;
            });

            // 🔹 Swagger
            services.AddSwaggerGen(options =>
            {
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                };

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    { securityScheme, new[] { "readAccess", "writeAccess" } }
                };

                options.AddSecurityDefinition("Bearer", securityScheme);
                options.AddSecurityRequirement(securityRequirement);

                options.CustomSchemaIds(type => type.FullName);
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerWithUI(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                        $"Monster API {description.GroupName.ToUpperInvariant()}");
                }
                options.RoutePrefix = "swagger"; // URL: /swagger
            });

            return app;
        }
    }
}
