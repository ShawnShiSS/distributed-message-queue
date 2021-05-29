using DMQ.MessageContracts;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.Consumers
{
    /// <summary>
    ///     Consumer for faulted messages.
    /// </summary>
    public class FaultConsumer :
        IConsumer<Fault<IFulfillOrder>>
    {
        public Task Consume(ConsumeContext<Fault<IFulfillOrder>> context)
        {
            throw new NotImplementedException();
        }
    }
}
