using System.Reflection;

using Asp.Versioning;

using Common.Packages.Logger.Dependency;
using Common.Packages.Response.Dependency;
using Common.Packages.Response.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using Serilog;

using TwitchAI.Api.Middleware;
using TwitchAI.Application.Constants;
using TwitchAI.Infrastructure;
using TwitchAI.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using static Common.Packages.Logger.Dependency.ServiceCollectionExtensions;

namespace TwitchAI.Api;

/// <summary>
/// Main class for dependency injection
/// </summary>
public static class Dependency
{
    internal static void AddAppSettingsConfiguration(this WebApplicationBuilder builder)
    {
        Execute(() =>
        {
            builder.Configuration
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();

            builder.Services.ConfigureLSCommonLogger(builder.Configuration);
            builder.Logging.AddSerilog(Log.Logger, dispose: true);
        });
    }

    /// <summary>
    /// Add all Api dependencies from solution
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddApiDependency(this IServiceCollection services, IConfiguration configuration)
    {
        return ExecuteReturn(() =>
        {
            services.AddAppMvc()
                    .AddJwtAuth(configuration)
                    .AddAppVersioning()
                    .AddSwaggerDocs(Assembly.GetExecutingAssembly());

            return services;
        });
    }

    private static IServiceCollection AddAppMvc(this IServiceCollection services)
    {
        return ExecuteReturn(() =>
        {
            services.AddEndpointsApiExplorer()
                    .AddControllers(options => { options.ReturnHttpNotAcceptable = true; })
                    .AddJsonOptions(o => { o.JsonSerializerOptions.IncludeFields = true; })
                    .AddNewtonsoftJson(cfg =>
                     {
                         cfg.SerializerSettings.ContractResolver = LsCommonConstants.JsOptions.ContractResolver;
                         cfg.SerializerSettings.ReferenceLoopHandling = LsCommonConstants.JsOptions.ReferenceLoopHandling;
                         cfg.SerializerSettings.NullValueHandling = LsCommonConstants.JsOptions.NullValueHandling;
                         cfg.SerializerSettings.Culture = LsCommonConstants.JsOptions.Culture;
                         cfg.SerializerSettings.Formatting = LsCommonConstants.JsOptions.Formatting;

                         foreach (var converter in LsCommonConstants.JsOptions.Converters)
                         {
                             cfg.SerializerSettings.Converters.Add(converter);
                         }
                     });

            return services;
        });
    }

    private static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        return ExecuteReturn(() =>
        {
            services.AddDataProtection();
            services.Configure<TwitchAI.Application.Models.JwtConfiguration>(configuration.GetSection("Jwt"));
            var jwt = new TwitchAI.Application.Models.JwtConfiguration();
            configuration.GetSection("Jwt").Bind(jwt);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwt.Secret))
                };
            });

            services.AddAuthorization();
            return services;
        });
    }

    /// <summary>
    /// Adds API versioning to the service collection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    private static IServiceCollection AddAppVersioning(this IServiceCollection services)
    {
        return ExecuteReturn(() =>
        {
            services.AddApiVersioning(cfg =>
            {
                cfg.DefaultApiVersion = new ApiVersion(1, 0);
                cfg.AssumeDefaultVersionWhenUnspecified = true;
                cfg.ReportApiVersions = true;
                cfg.ApiVersionReader = new UrlSegmentApiVersionReader();
            });
            return services;
        });
    }

    /// <summary>
    /// Добавляет сервисы для генерации документации Swagger.
    /// </summary>
    /// <param name="services">Коллекция сервисов, в которую будут добавлены сервисы Swagger.</param>
    /// <param name="assembly">Сборка с контроллерами, из которых будут извлечены версии API.</param>
    /// <returns>Коллекция сервисов (IServiceCollection) для цепочечного вызова.</returns>
    private static IServiceCollection AddSwaggerDocs(this IServiceCollection services, Assembly assembly)
    {
        return ExecuteReturn(() =>
        {
            services.AddSwaggerGen(c =>
            {
                // Извлекаем версии API из пространств имён контроллеров
                var apiVersions = GetApiVersionsFromControllers(assembly);

                // Проходим по каждой версии и добавляем её в SwaggerDoc
                foreach (var version in apiVersions)
                {
                    // Автоматически добавляем версию в Swagger
                    c.SwaggerDoc($"v{version}.0", new OpenApiInfo
                    {
                        Title = $"{Constants.ServiceName} API",
                        Version = $"v{version}.0"
                    });
                }

                // Подключаем XML-комментарии из указанных файлов для улучшения документации Swagger
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TwitchAI.Api.xml"));
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TwitchAI.Domain.xml"));

                // Устанавливаем схему для идентификаторов моделей в Swagger, убирая постфикс "ViewModel"
                c.CustomSchemaIds(type =>
                {
                    var returnedValue = type.Name;
                    if (returnedValue.EndsWith("Dto")) returnedValue = returnedValue.Replace("Dto", string.Empty);
                    if (returnedValue.EndsWith("ViewModel")) returnedValue = returnedValue.Replace("ViewModel", string.Empty);
                    if (returnedValue.EndsWith("Response")) returnedValue = returnedValue.Replace("Response", string.Empty);
                    if (returnedValue.EndsWith("Request")) returnedValue = returnedValue.Replace("Request", string.Empty);
                    return returnedValue;
                });

                // Переопределяем схему идентификаторов моделей, чтобы использовать полное имя типа
                c.CustomSchemaIds(type => type.ToString());

                #region -- security-scheme

                // Указываем Swagger использовать Bearer-схему по умолчанию для всех запросов
                //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                //{
                //    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                //    Name = "Authorization",
                //    In = ParameterLocation.Header,
                //    Type = SecuritySchemeType.ApiKey
                //});
                //c.AddSecurityRequirement(new OpenApiSecurityRequirement
                //{
                //    {
                //        new OpenApiSecurityScheme
                //        {
                //            Reference = new OpenApiReference
                //            {
                //                Type = ReferenceType.SecurityScheme,
                //                Id = "Bearer"
                //            }
                //        },
                //        Array.Empty<string>()
                //    }
                //});

                #endregion
            });
            return services;
        });
    }

    /// <summary>
    /// Using all Api dependencies
    /// </summary>
    /// <param name="app"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseApiDependencies(this WebApplication app, Assembly assembly)
    {
        return ExecuteReturn(() =>
        {
            app.UseLSCommonLocalization();
            app.UseLSCommonErrorHandler();

            app.UseMiddleware<RequestIdMiddleware>();

            app.UseRouting();
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapDefaultEndpoints();
            app.UseSwaggerDocs(assembly);

            return app;
        });
    }

    /// <summary>
    /// Configures the application to use Swagger documentation.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <param name="assembly">assembly for Api controller documentation</param>
    /// <returns>The application builder for chaining.</returns>
    private static IApplicationBuilder UseSwaggerDocs(this WebApplication app, Assembly assembly)
    {
        return ExecuteReturn(() =>
        {
            var apiVersions = GetApiVersionsFromControllers(assembly);
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger()
                   .UseSwaggerUI(c =>
                    {
                        foreach (var version in apiVersions)
                        {
                            c.SwaggerEndpoint($"/swagger/v{version}.0/swagger.json", $"{Constants.ServiceName} v{version}");
                        }
                    });
            }
            return app;
        });
    }

    #region -- private-methods --

    /// <summary>
    /// Метод для динамического получения версий API из контроллеров с поддержкой версии "vX.X"
    /// </summary>
    private static List<string> GetApiVersionsFromControllers(Assembly assembly)
    {
        var versions = new HashSet<string>(); // Используем HashSet для исключения дубликатов

        // Проходим по всем типам в сборке
        foreach (var type in assembly.GetTypes())
        {
            // Проверяем, является ли тип контроллером и имеет ли пространство имен
            if (typeof(ControllerBase).IsAssignableFrom(type) && type.Namespace != null)
            {
                // Разбиваем пространство имен на сегменты
                var namespaceSegments = type.Namespace.Split('.');

                // Ищем сегмент, который начинается с "v" и содержит версию
                var versionSegment = namespaceSegments.FirstOrDefault(segment => segment.StartsWith("v"));
                if (versionSegment != null)
                {
                    // Удаляем префикс "v" и добавляем версию в список
                    var version = versionSegment.Substring(1); // Убираем "v"
                    versions.Add(version); // HashSet автоматически предотвращает дубликаты
                }
            }
        }

        // Преобразуем HashSet в список и сортируем его
        var sortedVersions = versions.ToList();
        sortedVersions.Sort(); // Стандартная сортировка ("1", "1.1", "2", "2.1")

        return sortedVersions;
    }

    #endregion
}
