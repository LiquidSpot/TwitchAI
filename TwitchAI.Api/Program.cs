using System.Diagnostics;

using Common.Packages.Response.Exceptions;

using Serilog;

using TwitchAI.Api;
using TwitchAI.Application;
using TwitchAI.Infrastructure;
using TwitchAI.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
var activity = Activity.Current ?? new Activity(nameof(TwitchAI.Api)).Start();

try
{
    builder.AddAppSettingsConfiguration();
    builder.AddServiceDefaults();

    builder.Services
           .AddApiDependency(builder.Configuration)
           .AddApplicationDependency(builder.Configuration)
           .AddInfrastrucutreDependency(builder.Configuration);

    var app = builder.Build();

    app.UseApiDependencies(typeof(Program).Assembly);
    app.UseInfrastrucutreDependency(builder.Configuration);
    app.MapControllers();

    Log.Information(
        $"- Application started - [{builder.Environment.ApplicationName} " +
        $"{builder.Environment.EnvironmentName} " +
        $"{Environment.MachineName} " +
        $"{Environment.ProcessId}]");

    app.Run();
}
catch(Exception ex)
{
    activity.Stop();
    ExceptionFormatter.Format(ex);
    throw;
}
finally
{
    activity.Stop();
}