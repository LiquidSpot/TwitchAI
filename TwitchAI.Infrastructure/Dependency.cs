using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Castle.DynamicProxy;

using Common.Packages.HttpClient.Dependency;
using Common.Packages.HttpClient.Models;
using Common.Packages.Logger.Dependency;
using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Dependency;
using Common.Packages.Response.Exceptions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Serilog;

using TwitchAI.Application.Behaviors;
using TwitchAI.Application.Constants;
using TwitchAI.Application.Interfaces;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Application.Models;
using TwitchAI.Domain.Enums.ErrorCodes;
using TwitchAI.Domain.SR;
using TwitchAI.Infrastructure.Persistence.Repositories;
using TwitchAI.Infrastructure.Services;

using TwitchLib.Client.Models;

using static Common.Packages.Logger.Dependency.ServiceCollectionExtensions;

using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
#pragma warning disable CS4014

namespace TwitchAI.Infrastructure;

public static class Dependency
{
    public static IServiceCollection AddInfrastrucutreDependency(this IServiceCollection services, IConfiguration configuration)
    {
        return ExecuteReturn(() =>
        {
            services.AddCommonServices(configuration)
                    .AddServices(configuration);

            return services;
        });
    }

    internal static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        return ExecuteReturn(() =>
        {
            services.AddDataBaseConfiguration(configuration);

            services.AddSingleton<IBotRoleService, BotRoleService>();
            services.AddSingleton<ISoundAlertsService, SoundAlertsService>();

            services.AddScoped<IGreetingService, GreetingService>();
            services.AddScoped<IOpenAiService, OpenAiService>();
            services.AddScoped<IChatMessageService, ChatMessageService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<ITwitchUserService, TwitchUserService>();
            services.AddScoped<IUserMessageParser, UserMessageParser>();
            services.AddScoped<ITwitchIntegrationService, TwitchIntegrationService>();
            services.AddScoped<IViewerMonitoringService, ViewerMonitoringService>();
            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<IReplyLimitService, ReplyLimitService>();
            services.AddScoped<IEngineService, EngineService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserSettingsService, UserSettingsService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IIntegrationCheckService, IntegrationCheckService>();
            services.AddScoped<ICredentialProtector, CredentialProtector>();
            services.AddScoped<IBotSettingsService, BotSettingsService>();
            services.AddHostedService<ViewerMonitoringBackgroundService>();

            return services;
        });
    }

    internal static IServiceCollection AddDataBaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        return ExecuteReturn(() =>
        {
            services.AddDbContext<ApplicationDbContext>(
                options => DbContextConfigurator.ConfigureApplicationDbContext(options as DbContextOptionsBuilder<ApplicationDbContext>, configuration));

            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>))
                    .AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        });
    } 

    internal static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration configuration)
    {
        var errorMap = new HttpClientErrorMap
        {
            IncorrectRequest = BaseErrorCodes.IncorrectRequest,
            ErrorOnNullData = BaseErrorCodes.NoData,
            ErrorOnException = BaseErrorCodes.InternalServerError,
            PollyErrorEnum = BaseErrorCodes.ConnectionTimeout
        };

        return ExecuteReturn(() =>
        {
            var configs = configuration.GetSection(nameof(AuthClientsConfig)).Get<AuthClientsConfig>();

            services.AddLSCommonLogger()
                    .AddLSCommonResponse()
                    .AddHttpContextAccessor()
                    .AddLSHttpOptions(configuration)
                    .AddLSCommonLocalization<SRErrorCodes>()
                    .AddLSHttpClientConfiguration(configuration, errorMap);

            var polly = new PollyPolicies(3, 2, 60);
            services.AddLSHttpClientFactory(Constants.OpenAiClientKey, configs, polly);
            services.AddLSHttpClientFactory(Constants.TwitchApiClientKey, configs, polly);
            services.AddLSHttpClientFactory(Constants.OpenHolidaysApiClientKey, configs, polly);

            return services;
        });
    }

    public static WebApplication UseInfrastrucutreDependency(this WebApplication app, IConfiguration configuration)
    {
        return ExecuteReturn(() =>
        {
            app.MigrateDatabaseAsync(configuration);
            app.SeedAdminUserAsync(configuration);
            app.UseTwtichIntegration(configuration);
            return app;
        });
    }

    public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app, IConfiguration configuration)
    {
        using var scope = app.Services.CreateScope();
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var appCfg = configuration.GetSection(nameof(AppConfiguration)).Get<AppConfiguration>();
            if (appCfg is { MigrateDb: true })
            {
                await dbContext.Database.MigrateAsync().ConfigureAwait(false);
                Log.Information($"[{Activity.Current.Id}] Database migrated successfully. Exit from Application!");
                Environment.Exit(0);
            }
            Log.Information($"[{Activity.Current.Id}] Database migration throw passed.");
        }
        catch (Exception ex)
        {
            throw new LSException(BaseErrorCodes.InternalServerError, ex.Message);
        }

        return app;
    }

    [SuppressMessage("ReSharper", "AsyncConverter.ConfigureAwaitHighlighting")]
    internal static async Task<WebApplication> UseTwtichIntegration(this WebApplication app, IConfiguration configuration)
    {
        return await ExecuteReturn(async () =>
        {
            try
            {
                var lifetimeScope = app.Services.CreateAsyncScope();

                var twitch = lifetimeScope.ServiceProvider.GetRequiredService<ITwitchIntegrationService>();
                var twitchConfig = lifetimeScope.ServiceProvider.GetRequiredService<IOptions<TwitchConfiguration>>();
                var creds = new ConnectionCredentials(twitchConfig.Value.BotUsername, twitchConfig.Value.BotAccessToken);

                await twitch.InitializeClientAsync(creds);

                app.Lifetime.ApplicationStopping.Register(() => lifetimeScope.Dispose());

                return app;
            }
            catch (Exception ex)
            {
                throw new LSException(BaseErrorCodes.InternalServerError, ExceptionFormatter.Format(ex));
            }
        });
    }

    public static async Task<WebApplication> SeedAdminUserAsync(this WebApplication app, IConfiguration configuration)
    {
        using var scope = app.Services.CreateScope();
        try
        {
            var email = configuration["Admin:Email"] ?? "admin@local";
            var password = configuration["Admin:Password"] ?? "admin12345";
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await userService.RegisterAsync(email, password, cts.Token).ConfigureAwait(false);
            Log.Information($"[{Activity.Current?.Id}] Admin seed ensured for {email}");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Admin seed failed");
        }

        return app;
    }


    public static IServiceCollection AddProxiedScoped<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddScoped(provider =>
        {
            var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var logger = provider.GetRequiredService<IExternalLogger<LoggingInterceptor>>();

            // Создаем реализацию TImplementation напрямую через ActivatorUtilities,
            // не пытаясь получить её из контейнера как зарегистрированный сервис.
            var realService = ActivatorUtilities.CreateInstance<TImplementation>(provider);
            var LogInterceptor = new LoggingInterceptor(logger);

            // Адаптер, приводящий IAsyncInterceptor к IInterceptor
            var loggingAdapter = new AsyncDeterminationInterceptor(LogInterceptor);
            return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(realService, loggingAdapter);
        });

        return services;
    }
}