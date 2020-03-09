using System;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Extensions.Hosting;

namespace MasstransitHost
{
    public static class SerilogWebHostBuilderExtensions
    {
        /// <summary>
        /// Sets Serilog as the logging provider.
        /// Copied from https://github.com/serilog/serilog-aspnetcore/blob/dev/src/Serilog.AspNetCore/SerilogWebHostBuilderExtensions.cs
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="logger">The Serilog logger; if not supplied, the static <see cref="Serilog.Log"/> will be used.</param>
        /// <param name="dispose">When true, dispose <paramref name="logger"/> when the framework disposes the provider. If the
        /// logger is not specified but <paramref name="dispose"/> is true, the <see cref="Log.CloseAndFlush()"/> method will be
        /// called on the static <see cref="Log"/> class instead.</param>
        /// <param name="providers">A <see cref="LoggerProviderCollection"/> registered in the Serilog pipeline using the
        /// <c>WriteTo.Providers()</c> configuration method, enabling other <see cref="ILoggerProvider"/>s to receive events. By
        /// default, only Serilog sinks will receive events.</param>
        /// <returns>The host builder.</returns>
        public static IHostBuilder UseSerilog(
            this IHostBuilder builder,
            Serilog.ILogger logger = null,
            bool dispose = false,
            LoggerProviderCollection providers = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(collection =>
            {
                if (providers != null)
                {
                    collection.AddSingleton<ILoggerFactory>(services =>
                    {
                        var factory = new SerilogLoggerFactory(logger, dispose, providers);

                        foreach (var provider in services.GetServices<ILoggerProvider>())
                            factory.AddProvider(provider);

                        return factory;
                    });
                }
                else
                {
                    collection.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory(logger, dispose));
                }

                ConfigureServices(collection, logger);
            });

            return builder;
        }

        public static IHostBuilder UseSerilog(
            this IHostBuilder builder,
            Action<HostBuilderContext, LoggerConfiguration> configureLogger,
            bool preserveStaticLogger = false,
            bool writeToProviders = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureLogger == null) throw new ArgumentNullException(nameof(configureLogger));

            builder.ConfigureServices((context, collection) =>
            {
                var loggerConfiguration = new LoggerConfiguration();

                LoggerProviderCollection loggerProviders = null;
                if (writeToProviders)
                {
                    loggerProviders = new LoggerProviderCollection();
                    loggerConfiguration.WriteTo.Providers(loggerProviders);
                }

                configureLogger(context, loggerConfiguration);
                var logger = loggerConfiguration.CreateLogger();

                Serilog.ILogger registeredLogger = null;
                if (preserveStaticLogger)
                {
                    registeredLogger = logger;
                }
                else
                {
                    // Passing a `null` logger to `SerilogLoggerFactory` results in disposal via
                    // `Log.CloseAndFlush()`, which additionally replaces the static logger with a no-op.
                    Log.Logger = logger;
                }

                collection.AddSingleton<ILoggerFactory>(services =>
                {
                    var factory = new SerilogLoggerFactory(registeredLogger, true, loggerProviders);

                    if (writeToProviders)
                    {
                        foreach (var provider in services.GetServices<ILoggerProvider>())
                            factory.AddProvider(provider);
                    }

                    return factory;
                });

                ConfigureServices(collection, logger);
            });
            return builder;
        }

        static void ConfigureServices(IServiceCollection collection, Serilog.ILogger logger)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            if (logger != null)
            {
                // This won't (and shouldn't) take ownership of the logger. 
                collection.AddSingleton(logger);
            }

            // Registered to provide two services...
            var diagnosticContext = new DiagnosticContext(logger);

            // Consumed by e.g. middleware
            collection.AddSingleton(diagnosticContext);

            // Consumed by user code
            collection.AddSingleton<IDiagnosticContext>(diagnosticContext);
        }
    }
}
