using Automatonymous;
using MassTransit.Saga;
using System;

namespace DMQ.MessageComponents.StateMachines
{
    public class OrderState : 
        SagaStateMachineInstance,
        ISagaVersion 
    {
        /// <summary>
        ///     Unique identifier to identify the saga instance.
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        ///     Redis version check at run time.
        ///     Redis uses optimistic concurrency and requires this.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        ///     Current state of the saga instance.
        /// </summary>
        public string CurrentState { get; set; }

        /// <summary>
        ///     Customer number.
        /// </summary>
        public string CustomerNumber { get; set; }

        /// <summary>
        ///     Order submitted date.
        /// </summary>
        public DateTime? SubmittedDate { get; set; }

        /// <summary>
        ///     Updated timestamp
        /// </summary>
        public DateTime Updated { get; set; }
        
    }
}
