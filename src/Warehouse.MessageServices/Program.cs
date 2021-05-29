using MassTransit;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Threading.Tasks;
using Warehouse.MessageComponents;
using Warehouse.MessageComponents.Consumers;
using Warehouse.MessageComponents.StateMachines;
using Warehouse.MessageContracts;

namespace Warehouse.MessageServices
{
    /// <summary>
    ///     Program.
    ///     Note this will .NET Core Generic Host, which makes it easy to run the console app as a Windows Service or even Linux Demon.
    /// </summary>
    class Program
    {
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
                    // Use snake-like-names for queues.
                    // E.g., queue for SubmitOrder messages would be named "submit-order"
                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
                    services.AddMassTransit(cfg =>
                    {
                        // Allow Mass Transit to scan all the types instead of manually adding them all
                        cfg.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();

                        // Add RabbitMQ as the message broker
                        cfg.UsingRabbitMq(ConfigureBus);

                        // Add Azure Service Bus as the message broker
                        //cfg.UsingAzureServiceBus((busRegistrationContext, busFactoryConfigurator) =>
                        //{
                        //    // TODO: Connection string should go into appsettings.
                        //    busFactoryConfigurator.Host("ServiceBusConnectionString");
                        //});


                        // Add Saga and its repository
                        const string mongoConfigurationString = "mongodb://127.0.0.1";
                        cfg.AddSagaStateMachine<AllocationStateMachine, AllocationState>(typeof(AllocationStateMachineDefinition))
                            .MongoDbRepository(r => 
                            {
                                r.Connection = mongoConfigurationString;
                                r.DatabaseName = "allocations";
                            });

                    });

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

            Log.CloseAndFlush();

        }

        static void ConfigureBus(IBusRegistrationContext busRegistrationContext, IRabbitMqBusFactoryConfigurator configurator)
        {
            // Tell the bus to use the quartz message scheduler
            configurator.UseMessageScheduler(new System.Uri("queue:quartz"));

            configurator.ConfigureEndpoints(busRegistrationContext);
        }
    }
}

