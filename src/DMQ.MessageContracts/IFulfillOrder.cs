using System;

namespace DMQ.MessageContracts
{
    public interface IFulfillOrder
    {
        Guid OrderId { get; }
    }
}
