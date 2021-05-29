using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;

namespace DMQ.MessageComponents.Consumers
{
    /// <summary>
    ///     Consumer definition.
    ///     The name is not important, Mass Transit matches it by type.
    /// </summary>
    public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
    {
        public SubmitOrderConsumerDefinition()
        {
            //EndpointName = "submit.order.todo";
            ConcurrentMessageLimit = 8;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator)
        {
            // Add custom configuration to the pipeline middleware

            // When exception occurs, retry 3 times before we move the message out of the queue and put it to the error queue.
            endpointConfigurator.UseMessageRetry(r => r.Interval(3, 1000));

        }
    }
}
