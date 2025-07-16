using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace TwitchAI.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    // ���� ����� � ����� ��������� ������-����� �������� ������������:
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // ����������� OpenTelemetry-�����������, ������� � ��������.
        builder.ConfigureOpenTelemetry();

        // ��������� ������� health checks (�������� �������� � ����������).
        builder.AddDefaultHealthChecks();

        ///���������� ���������� ������-���������.
        /// ��� ��������� ��� ������ ������� �������� ����� ����������� ���������� ������
        /// � ������������ ��������� ���������/��������, �� ��������� ��������� ������ ��������.
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // �������� ����������� ��������� ������������ (retry, circuit breaker � �. �.).
            http.AddStandardResilienceHandler();

            // ����������� ������-��������� ��� ���� HTTP-�������� �� ���������.
            http.AddServiceDiscovery();
        });

        // ��� ��������, ��� ��� ��������� ����� ������-��������� ����� ���������� ����������� �������
        builder.Services.Configure<ServiceDiscoveryOptions>(options => { options.AllowedSchemes = new[] { "https", "http" }; });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
               .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                           .AddHttpClientInstrumentation()
                           .AddRuntimeInstrumentation()
                           .AddNpgsqlInstrumentation();
                })
               .WithTracing(tracing =>
                {
                    tracing.AddSource(builder.Environment.ApplicationName)
                           .AddAspNetCoreInstrumentation()
                           .AddHttpClientInstrumentation()
                           .AddEntityFrameworkCoreInstrumentation()
                           .AddNpgsql();
                });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// ���� ����� ������ ConfigureOpenTelemetry() �������� ������ �� ����������� ���������� ����������� (���� ������ ������ �� ��������� ����������):
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter) { builder.Services.AddOpenTelemetry().UseOtlpExporter(); }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // ��������� ������� Health Checks (����� AddHealthChecks()):
        /* �������� ��������: "self".
         * ���������� ������ HealthCheckResult.Healthy(). 
         * ����� ��� "live", ������� ����� ����� ������������ ��� �������� /alive.
         * � �������� ���������� ����� ��������� �������������� �������� (��������, ���������� � ���� ������, ����������� ������� �������� � �. �.).
         */
        builder.Services.AddHealthChecks()
               .AddCheck("self", () => HealthCheckResult.Healthy(), new[] { "live" });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // ����������� �������� ����� (endpoints) ��� health checks:
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
