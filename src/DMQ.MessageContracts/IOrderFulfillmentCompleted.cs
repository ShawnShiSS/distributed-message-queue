using System;

namespace DMQ.MessageContracts
{
    public interface IOrderFulfillmentCompleted
    {
        Guid OrderId { get; }

        DateTime Timestamp { get; }
    }
}
