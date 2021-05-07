using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.Consumers
{
    /// <summary>
    ///     A catch-all listener to the routing slip events.
    /// </summary>
    public class RoutingSlipEventConsumer :
        IConsumer<RoutingSlipCompleted>,
        IConsumer<RoutingSlipActivityCompleted>,
        IConsumer<RoutingSlipFaulted>
    {
        public ILogger<RoutingSlipEventConsumer> _logger { get; }

        public RoutingSlipEventConsumer(ILogger<RoutingSlipEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<RoutingSlipCompleted> context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.Log(LogLevel.Information, "Routing Slip Completed: {TrackingNumber}", context.Message.TrackingNumber);
            }

            // Not an async state machine.
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoutingSlipActivityCompleted> context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.Log(LogLevel.Information, "Routing Slip Activity Completed: {TrackingNumber} {ActivityName}", context.Message.TrackingNumber, context.Message.ActivityName);
            }

            // Not an async state machine.
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoutingSlipFaulted> context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.Log(LogLevel.Information, "Routing Slip Completed: {TrackingNumber} {ExceptionInfo}", context.Message.TrackingNumber, context.Message.ActivityExceptions.FirstOrDefault());
            }

            // Not an async state machine.
            return Task.CompletedTask;
        }
    }
}
