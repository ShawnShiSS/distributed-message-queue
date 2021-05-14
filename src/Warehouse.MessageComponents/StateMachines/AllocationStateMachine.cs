using Automatonymous;
using MassTransit;
using System;
using Warehouse.MessageContracts;

namespace Warehouse.MessageComponents.StateMachines
{
    public class AllocationStateMachine :
        MassTransitStateMachine<AllocationState>
    {
        public AllocationStateMachine()
        {
            Event(() => AllocationCreated, x => x.CorrelateById(m => m.Message.AllocationId));
            Event(() => AllocationReleaseRequested, x => x.CorrelateById(m => m.Message.AllocationId));

            // After scheduling, a token is returned and the token can be used to cancel a scheduled event.
            Schedule(() => HoldExpiration, x => x.HoldDurationToken, s => 
            {
                s.Delay = TimeSpan.FromHours(2);
                // Allocation id is used to correlate back to the state machine
                s.Received = m => m.CorrelateById(p => p.Message.AllocationId);
            });

            // Property of the state class to use to store the state
            InstanceState(x => x.CurrentState);

            Initially(
                When(AllocationCreated)
                    // after allocation is created, let's schedule another event, which will be sent to queue "quartz"
                    // Events in the "quartz" queue will be consumed by a scheduler, which is a separate hosted service. The scheduler will persist the scheduled events in a database like SQL Server.
                    // Note: the scheduler hosted service is NOT part of the solution (yet).
                    .Schedule(HoldExpiration, 
                              context => context.Init<IAllocationHoldDurationExpired>(new { context.Data.AllocationId}), 
                              context => context.Data.HoldDuration)
                    .TransitionTo(Allocated),
                // In case the release request gets in before the allocation is created
                When(AllocationReleaseRequested)
                    .TransitionTo(Released)
            );

            During(Released,
                When(AllocationCreated)
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Allocation was already released: {context.Instance.CorrelationId}"))
                    .Finalize()
            );

            During(Allocated,
                When(HoldExpiration.Received)
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Allocation expired and is released: {context.Instance.CorrelationId}"))
                    //.TransitionTo(Released)
                    // instead of releasing it and keeping a log, let's just finalize the state machine instance
                    .Finalize(),
                When(AllocationReleaseRequested)
                    .ThenAsync(context => Console.Out.WriteLineAsync($"Allocation release request, granted: {context.Instance.CorrelationId}"))
                    //.TransitionTo(Released)
                    // instead of releasing it and keeping a log, let's just finalize the state machine instance
                    .Finalize(),
                // In case RabbitMQ connection dropped after allocation is created by before the scheduler finishes.
                When(AllocationCreated)
                    .Schedule(HoldExpiration,
                              context => context.Init<IAllocationHoldDurationExpired>(new { context.Data.AllocationId }),
                              context => context.Data.HoldDuration)
                    .TransitionTo(Allocated)
            );

            // Tell saga to delete this instance when the instance is finalized.
            SetCompletedWhenFinalized();
        }

        public Schedule<AllocationState, IAllocationHoldDurationExpired> HoldExpiration { get; set; }


        public State Allocated { get; set; }
        public State Released { get; set; }

        public Event<IAllocationCreated> AllocationCreated { get; set; }
        public Event<IAllocationReleaseRequested> AllocationReleaseRequested { get; set; }
    }
}
