using Automatonymous;
using DMQ.MessageContracts;
using GreenPipes;
using System;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.StateMachines.OrderStateMachineActivities
{
    /// <summary>
    ///     Activity to run on order accepted event.
    ///     Custom activity allows us to inject dependancies like database context here, instead of putting dependancies on the state machine. The state machine should have no dependancy, otherwise it becomes hard to unit test, particularly when where are scoped dependancies.
    /// </summary>
    public class AcceptOrderActivity : Activity<OrderState, IOrderAccepted>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, IOrderAccepted> context, Behavior<OrderState, IOrderAccepted> next)
        {
            // Do anything you want
            Console.WriteLine($"Execute for order id = {context.Data.OrderId}");
            // Call next in the pipeline
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, IOrderAccepted, TException> context, Behavior<OrderState, IOrderAccepted> next) where TException : Exception
        {
            // Have to call next in order for the pipeline to support unroll.
            return next.Faulted(context);
        }


        /// <summary>
        ///     For discovery
        /// </summary>
        /// <param name="context"></param>
        public void Probe(ProbeContext context)
        {
            context.CreateScope("accept-order");
        }
    }
}
