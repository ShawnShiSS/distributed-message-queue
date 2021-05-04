using Automatonymous;
using DMQ.MessageContracts;
using MassTransit;
using System;

namespace DMQ.MessageComponents.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            // State machine events have to be correlated to the state machine instance.
            // E.g., OrderId in the OrderSubmitted event will be used to correlate the event to a saga instance. If the instance does not exist, it will get created.
            // If Redis is used as the state machine repository, redis-cli allows us to check an order state by "get orderId"
            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => CheckOrderStatus, x =>
            {
                x.CorrelateById(m => m.Message.OrderId);
                // If state instance does not exist for the order id, we need to return not found.
                x.OnMissingInstance(m => m.ExecuteAsync(async context => 
                { 
                    // Only respond if a response is expected
                    if (context.RequestId.HasValue)
                    {
                        await context.RespondAsync<IOrderNotFound>(new 
                        { 
                            OrderId = context.Message.OrderId
                        });
                    }
                }));
            });

            InstanceState(x => x.CurrentState);

            // All state machines start in the initial state
            // When an OrderSubmitted event is published, OrderStateMachine uses it to create an instance of itself, identified by order id.
            Initially(
                When(OrderSubmitted)
                    // Assign the properties in the message (in Data) to the saga instance
                    .Then(context => 
                    {
                        context.Instance.CustomerNumber = context.Data.CustomerNumber;
                        context.Instance.Updated = DateTime.UtcNow;
                        context.Instance.SubmittedDate = context.Data.Timestamp;

                    })
                    .TransitionTo(Submitted)
            );

            // Messages are not guaranteed to be processed in order, so we need to handle out-of-order messages.
            // DuringAny includes all states other than initial state and final state.
            DuringAny(
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        context.Instance.CustomerNumber = context.Data.CustomerNumber;
                        context.Instance.SubmittedDate = context.Data.Timestamp;

                    })
            );

            // Handle check order status requests.
            DuringAny(
                When(CheckOrderStatus)
                    .RespondAsync(x => x.Init<IOrderStatus>(new
                    {
                        OrderId = x.Instance.CorrelationId,
                        State = x.Instance.CurrentState
                    }))
            );

            During(Submitted,
                // If the same order id gets submitted twice, the second request will fail with "Not accepted in state Submitted".
                Ignore(OrderSubmitted)
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

        /// <summary>
        ///     On check order status event.
        /// </summary>
        public Event<ICheckOrder> CheckOrderStatus { get; private set; }
    }
}
