using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using MassTransit;
using System.Threading;

namespace Warehouse.MessageServices
{
    public class MassTransitConsoleHostedService : IHostedService
    {
        private readonly IBusControl _bus;

        public MassTransitConsoleHostedService(IBusControl bus)
        {
            _bus = bus;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _bus.StopAsync(cancellationToken);
        }
    }
}
