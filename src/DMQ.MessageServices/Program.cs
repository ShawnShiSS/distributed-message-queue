using DMQ.MessageComponents.BatchConsumers;
using DMQ.MessageComponents.Consumers;
using DMQ.MessageComponents.CourierActivities;
using DMQ.MessageComponents.StateMachines;
using DMQ.MessageComponents.StateMachines.OrderStateMachineActivities;
using MassTransit;
using MassTransit.Courier.Contracts;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Warehouse.MessageContracts;

namespace DMQ.MessageServices
{
    /// <summary>
    ///     Program.
    ///     Note this will .NET Core Generic Host, which makes it easy to run the console app as a Windows Service or even Linux Demon.
    /// </summary>
    class Program
    {
        /// <summary>
        ///     Application Insight module.
        /// </summary>
        static DependencyTrackingTelemetryModule _module;
        /// <summary>
        ///     Application Insight telemetry client.
        /// </summary>
        static TelemetryClient _telemetryClient;

        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var isService = !(Debugger.IsAttached);

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) => 
                {
                    config.AddJsonFile("appsettings.json", true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostingContext, services) => 
                {
                    // Configure application insight 
                    // Note: "MT-Activity-id" in the message "headers" section is the diagnostic activity id for telemetry data so that App Insights can group the messages correctly.
                    _module = new DependencyTrackingTelemetryModule();
                    _module.IncludeDiagnosticSourceActivities.Add("MassTransit");
                    TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                    telemetryConfiguration.InstrumentationKey = "Your_InstrumentationKey_From_AzurePortal";
                    telemetryConfiguration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
                    _telemetryClient = new TelemetryClient(telemetryConfiguration);
                    _module.Initialize(telemetryConfiguration);

                    // Use snake-like-names for queues.
                    // E.g., queue for SubmitOrder messages would be named "submit-order"
                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

                    // Batch consumers do not get discovered by namespace.
                    services.AddScoped<RoutingSlipBatchEventConsumer>();
                    services.AddMassTransit(cfg => 
                    {
                        // Allow Mass Transit to scan all the types instead of manually adding them all
                        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                        // This should add activity, and also create two endpoints for the activity
                        // one for execute activity, one for compensate activity.
                        cfg.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();

                        // Add RabbitMQ as the message broker
                        cfg.UsingRabbitMq(ConfigureBus);

                        // Add Saga State Machines
                        //const string redisConfigurationString = "127.0.0.1";
                        const string mongoConfigurationString = "mongodb://127.0.0.1";
                        // Passing a definition allows us to configure 
                        cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                           // Redis repository to store state instances. By default, redis runs on localhost.
                           //.RedisRepository(r => 
                           //{
                           //    r.ConcurrencyMode = MassTransit.RedisIntegration.ConcurrencyMode.Optimistic;
                           //    r.DatabaseConfiguration(redisConfigurationString);
                           //})
                           // MongoDB repository to store state instances and support querying.
                           .MongoDbRepository(r =>
                           {
                               r.Connection = mongoConfigurationString;
                               r.DatabaseName = "orders";
                           });

                        // In order to call the warehouse to allocate inventory from the FulfillOrderConsumer.
                        cfg.AddRequestClient<IAllocateInventory>();
                    });

                    // This has to be registered as it is used in OrderStateMachine
                    services.AddScoped<AcceptOrderActivity>();

                    // This will also configure the message queue endpoints.
                    services.AddHostedService<MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) => 
                {
                    logging.AddSerilog(dispose: true);
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    //logging.AddConsole();
                });
            
            if (isService)
            {
                await builder.UseWindowsService()
                             .Build()
                             .RunAsync();
            }
            else
            {
                await builder.RunConsoleAsync();
            }

            // Application insight clean up
            _telemetryClient?.Flush();
            _module?.Dispose();

            Log.CloseAndFlush();
        }

        static void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
        {
            // Batch endpoints need to be handled manually
            configurator.ReceiveEndpoint(KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(), e =>
            {
                // Make sure we prefetch more than the message limit for batching.
                e.PrefetchCount = 16;

                e.Batch<RoutingSlipCompleted>(b => 
                {
                    // Time limit to wait
                    b.TimeLimit = TimeSpan.FromSeconds(5);

                    // Max number of messages to wait
                    b.MessageLimit = 10;

                    b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(context);
                });
            });

            // Auto configure endpoints 
            configurator.ConfigureEndpoints(context);
        }
    }
}
