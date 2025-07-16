using System.Reflection;
using MediatR;
using MediatR.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using TwitchAI.Application.Behaviors;
using TwitchAI.Application.Models;

using static Common.Packages.Logger.Dependency.ServiceCollectionExtensions;

using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace TwitchAI.Application;

public static class Dependency
{
    public static IServiceCollection AddApplicationDependency(this IServiceCollection services, IConfiguration configuration)
    {
        return ExecuteReturn(() =>
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                                      .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.FullName))
                                      .ToArray();

            services.ConfigureMediatR(assemblies)
                    .AddAutoMapper(assemblies)
                    .AddApplicationOptions(configuration);

            return services;
        });
    }

    public static IServiceCollection ConfigureMediatR(this IServiceCollection services, Assembly[] assemblies)
    {
        return ExecuteReturn(() =>
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assemblies);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggerPipelineBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestExceptionProcessorBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CancellationTokenBehavior<,>));
            });

            return services;
        });
    }

    public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return ExecuteReturn(() =>
        {
            services.Configure<AppConfiguration>(configuration.GetSection(nameof(AppConfiguration)));
            services.Configure<TwitchConfiguration>(configuration.GetSection(nameof(TwitchConfiguration)));

            return services;
        });
    }
}