using System;

namespace DMQ.MessageContracts
{
    public interface IOrderFulfillmentFaulted
    {
        Guid OrderId { get; }
        DateTime TimeStamp { get; }
    }


}
