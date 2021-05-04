using System;

namespace DMQ.MessageContracts
{
    public interface ICustomerAccountClosed
    {
        /// <summary>
        ///     Customer Id.
        /// </summary>
        Guid CustomerId { get; }
        /// <summary>
        ///     Business identifier, customer number.
        /// </summary>
        string CustomerNumber { get; }
    }
}
