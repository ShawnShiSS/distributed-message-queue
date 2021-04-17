using DMQ.MessageComponents.Consumers;
using MassTransit;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DMQ.MessageServices
{
    /// <summary>
    ///     Program.
    ///     Note this will .NET Core Generic Host, which makes it easy to run the console app as a Windows Service or even Linux Demon.
    /// </summary>
    partial class Program
    {
        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached) || args.Contains("--console");

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
                    // Use snake-like-names for queues
                    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
                    services.AddMassTransit(cfg => 
                    {
                        // Allow Mass Transit to scan all the types instead of manually adding them all
                        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();

                        cfg.UsingRabbitMq(ConfigureBus);
                    });

                    services.AddHostedService<MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) => 
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });
            
            if (isService)
            {
                await builder.UseWindowsService().Build().RunAsync();
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
