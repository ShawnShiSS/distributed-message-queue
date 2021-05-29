using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.BatchConsumers
{
    /// <summary>
    ///     Batch message consumer
    /// </summary>
    public class RoutingSlipBatchEventConsumer :
        IConsumer<Batch<RoutingSlipCompleted>>
    {
        public ILogger<RoutingSlipBatchEventConsumer> _logger { get; }
        public RoutingSlipBatchEventConsumer(ILogger<RoutingSlipBatchEventConsumer> logger)
        {
            _logger = logger;
        }


        public Task Consume(ConsumeContext<Batch<RoutingSlipCompleted>> context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {

                // This is not done by transport. MT will read a batch of messages and deliver the messages to the consumer. Messages are locked in the transport by MT.
                var trackingNumbers = context.Message.Select(x => x.Message.TrackingNumber);

                _logger.Log(LogLevel.Information, "Routing Slip Completed: {TrackingNumbers}", string.Join(" ,", trackingNumbers));

            }

            return Task.CompletedTask;
        }
    }
}
