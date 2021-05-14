using GreenPipes;
using MassTransit;
using MassTransit.Definition;

namespace Warehouse.MessageComponents.StateMachines
{
    /// <summary>
    ///     Definition for the saga.
    ///     This allows us to define how messages should be handled within the endpoints.
    /// </summary>
    public class AllocationStateMachineDefinition :
        SagaDefinition<AllocationState>
    {
        public AllocationStateMachineDefinition()
        {
            // Force endpoint name
            //this.EndpointName = "";

            ConcurrentMessageLimit = 4;

        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<AllocationState> sagaConfigurator)
        {
            // In order to deal with concurrency, i.e. two concurrent requests trying to create a saga, we want one to fail to retry after the other one has created the saga.
            // Retry an errored message by sending it back to the pipeline, which will reload the saga.
            // Retrying is done in-memory and keeps the message lock on the message broker, so do not retry too many times, otherwise the locked messages will block your message consumptions.
            endpointConfigurator.UseMessageRetry(r => r.Interval(3, 1000));

            // When faulted messages happen, hold the events in memory and do not send them until the persistance part of the transaction is completed. Otherwise, we get into odd cases where states are out of whack.
            endpointConfigurator.UseInMemoryOutbox();
        }


    }
}
