using Automatonymous;
using DMQ.MessageContracts;
using MassTransit.Saga;
using System;

namespace DMQ.MessageComponents.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            // State machine events have to be correlated to the state machine instance.
            // E.g., OrderId in the OrderSubmitted event will be used to correlate the event to a saga instance. If the instance does not exist, it will get created.
            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));

            InstanceState(x => x.CurrentState);

            // All state machines start in the initial state
            Initially(
                When(OrderSubmitted)
                    .TransitionTo(Submitted)
            );

            // Final state is specical, as state machines in final state will be removed
        }

        /// <summary>
        ///     Submitted state.
        /// </summary>
        public State Submitted { get; private set; }

        /// <summary>
        ///     On order submitted event.
        /// </summary>
        public Event<IOrderSubmitted> OrderSubmitted { get; private set; }
    }

    public class OrderState : 
        SagaStateMachineInstance,
        ISagaVersion 
    {
        /// <summary>
        ///     Unique identifier to identify the saga instance.
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        ///     Current state of the saga instance.
        /// </summary>
        public string CurrentState { get; set; }

        /// <summary>
        ///     Redis version check at run time.
        ///     Redis uses optimistic concurrency and requires this.
        /// </summary>
        public int Version { get; set; }
    }
}
