using System;

namespace DMQ.MessageContracts
{
    public interface IOrderStatus
    {
        Guid OrderId { get; }
        string State { get; }
    }
}
