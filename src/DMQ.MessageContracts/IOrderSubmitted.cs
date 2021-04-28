using System;

namespace DMQ.MessageContracts
{
    /// <summary>
    ///     Order submitted event contract.
    /// </summary>
    public interface IOrderSubmitted
    {
        /// <summary>
        ///     Unique identifier for an order
        /// </summary>
        Guid OrderId { get; }
        DateTime Timestamp { get; }
        string CustomerNumber { get; }
    }
}
