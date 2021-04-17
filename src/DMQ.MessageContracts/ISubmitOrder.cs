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
    }
}
