using System;

namespace DMQ.MessageContracts
{
    /// <summary>
    ///     Contract for SubmitOrder.
    ///     Note: if Mass Transit is used, MT will create a dynamic internal backing implementation of this interface.
    /// </summary>
    public interface ISubmitOrder
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
        string CustomerNumber { get; }

        /// <summary>
        ///     Payment card number.
        ///     In real world, instead of storing card numbers in messages, a fulfill order consumer can just use a reference or a token to request the payment information as a routing slip acitivity, from a PCI vault in order to be PCI compliant.
        /// </summary>
        string PaymentCardNumber { get; }
    }
}
