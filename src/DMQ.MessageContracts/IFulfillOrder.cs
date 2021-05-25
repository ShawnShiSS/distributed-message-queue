using System;

namespace DMQ.MessageContracts
{
    public interface IFulfillOrder
    {
        Guid OrderId { get; }
        string CustomerNumber { get; }
        string PaymentCardNumber { get; }

    }
}
