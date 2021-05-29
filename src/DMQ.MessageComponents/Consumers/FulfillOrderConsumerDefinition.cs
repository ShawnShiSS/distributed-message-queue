using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using System;
using System.Collections.Generic;
using System.Text;

namespace DMQ.MessageComponents.Consumers
{
    /// <summary>
    ///     Consumer definition.
    ///     The name is not important, Mass Transit matches it by type.
    /// </summary>
    public class FulfillOrderConsumerDefinition : ConsumerDefinition<FulfillOrderConsumer>
    {
        public FulfillOrderConsumerDefinition()
        {
            //EndpointName = "submit.order.todo";
            ConcurrentMessageLimit = 8;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<FulfillOrderConsumer> consumerConfigurator)
        {
            // Add custom configuration to the pipeline middleware

            // When exception occurs, retry 3 times before we move the message out of the queue and put it to the error queue.
            endpointConfigurator.UseMessageRetry(r =>
            {
                // Ignore unrecoverable errors
                r.Ignore<InvalidOperationException>();

                // normal retry
                r.Interval(3, 1000);
            });

            // It is possible to discard faulted messages and not send them to the xxx_error queue. Not recommended...
            //endpointConfigurator.DiscardFaultedMessages();
        }
    }
}
