﻿using DMQ.MessageComponents.Consumers;
using DMQ.MessageComponents.CourierActivities;
using DMQ.MessageComponents.StateMachines;
using DMQ.MessageComponents.StateMachines.OrderStateMachineActivities;
using MassTransit;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        static async Task Main(string[] args)
        {
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
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
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
        }

        static void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
        {
            configurator.ConfigureEndpoints(context);
        }
    }
}
