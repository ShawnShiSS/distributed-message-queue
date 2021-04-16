using System;

namespace DMQ.MessageContracts
{
    public interface ISubmitOrder
    {
        Guid OrderId { get; }

        DateTime Timestamp { get; }

        string CustomerNumber { get; }

    }
}
