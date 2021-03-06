using Automatonymous;
using DMQ.MessageComponents.StateMachines.OrderStateMachineActivities;
using DMQ.MessageContracts;
using MassTransit;
using System;
using System.Linq;

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
            Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => FulfillmentFaulted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => FulfillmentCompleted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => FulfillOrderFaulted, x => x.CorrelateById(m => m.Message.Message.OrderId));

            Event(() => CheckOrderStatusRequested, x =>
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

            // Example of event correlation without the actual CorrelationId.
            // Account closed event does not have an order id,
            // so we have to correlate the saga instance by the customer number.
            // Customer number has to be queried out to the saga repo to perform a search.
            // Note: Redis not not support queries that search by properties, which are required to look up saga instances by properties like customer number other than Saga Correlation Id.
            Event(() => AccountClosed, x => x.CorrelateBy((saga, context) => saga.CustomerNumber == context.Message.CustomerNumber));

            InstanceState(x => x.CurrentState);

            // All state machines start in the initial state
            // When an OrderSubmitted event is published, OrderStateMachine uses it to create an instance of itself, identified by order id.
            Initially(
                When(OrderSubmitted)
                    // Assign the properties in the message (in Data) to the saga instance
                    .Then(context => 
                    {
                        context.Instance.CustomerNumber = context.Data.CustomerNumber;
                        context.Instance.PaymentCardNumber = context.Data.PaymentCardNumber;
                        context.Instance.Updated = DateTime.UtcNow;
                        context.Instance.SubmittedDate = context.Data.Timestamp;

                    })
                    .TransitionTo(Submitted)
            );

            During(Accepted,
                When(FulfillOrderFaulted)
                    .Then(context => Console.WriteLine("Fulfill order faulted: {0}", context.Data.Exceptions.FirstOrDefault()?.Message))
                    .TransitionTo(Faulted),
                When(FulfillmentFaulted)
                    .TransitionTo(Faulted),
                When(FulfillmentCompleted)
                    .TransitionTo(Completed)
            );

            // Messages are not guaranteed to be processed in order, so we need to handle out-of-order messages.
            // DuringAny includes all states other than initial state and final state.
            DuringAny(
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        context.Instance.CustomerNumber = context.Data.CustomerNumber;
                        context.Instance.SubmittedDate = context.Data.Timestamp;

                    }),
                When(OrderAccepted)
                    // custom activity that should get triggered
                    .Activity(x => x.OfType<AcceptOrderActivity>())
                    .TransitionTo(Accepted)
            );

            // Handle check order status requests.
            DuringAny(
                When(CheckOrderStatusRequested)
                    .RespondAsync(x => x.Init<IOrderStatus>(new
                    {
                        OrderId = x.Instance.CorrelationId,
                        State = x.Instance.CurrentState
                    }))
            );

            During(Submitted,
                // If the same order id gets submitted twice, the second request will fail with "Not accepted in state Submitted".
                Ignore(OrderSubmitted),
                When(AccountClosed)
                    .TransitionTo(Canceled)
            );


            // Final state is specical, as state machines in final state will be removed
        }

        /// <summary>
        ///     Submitted state.
        /// </summary>
        public State Submitted { get; private set; }

        /// <summary>
        ///     Accepted state.
        /// </summary>
        public State Accepted { get; private set; }

        /// <summary>
        ///     Canceled state.
        /// </summary>
        public State Canceled { get; private set; }

        /// <summary>
        ///     Faulted state.
        /// </summary>
        public State Faulted { get; private set; }


        /// <summary>
        ///  Completed state
        /// </summary>
        public State Completed { get; private set; }

        /// <summary>
        ///     On order submitted event.
        /// </summary>
        public Event<IOrderSubmitted> OrderSubmitted { get; private set; }

        /// <summary>
        ///     On order accepted event.
        /// </summary>
        public Event<IOrderAccepted> OrderAccepted { get; private set; }

        /// <summary>
        ///     On check order status event.
        /// </summary>
        public Event<ICheckOrder> CheckOrderStatusRequested { get; private set; }

        /// <summary>
        ///     Completed event from the routing slip transaction being fully completed.
        /// </summary>
        public Event<IOrderFulfillmentCompleted> FulfillmentCompleted { get; private set; }

        /// <summary>
        ///     Faulted event from the routing slip not being able to fully complete in a transactional manner.
        /// </summary>
        public Event<IOrderFulfillmentFaulted> FulfillmentFaulted { get; private set; }

        /// <summary>
        ///     On account closed event, we may want to do things like cancel an order.
        /// </summary>
        public Event<ICustomerAccountClosed> AccountClosed { get; private set; }

        /// <summary>
        ///     Fault message when error occurs in the fulfill order consumer.
        /// </summary>
        public Event<Fault<IFulfillOrder>> FulfillOrderFaulted { get; set; }
    }
}
