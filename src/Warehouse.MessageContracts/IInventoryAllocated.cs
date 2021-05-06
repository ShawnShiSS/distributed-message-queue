using System;

namespace Warehouse.MessageContracts
{
    public interface IInventoryAllocated
    {
        /// <summary>
        ///     Identifier, generated in advance.
        ///     This is reference from the source application to ensure idempotency across domain boundary.
        /// </summary>
        Guid AllocationId { get; }
        string ItemNumber { get; }
        decimal Quantity { get; }
    }
}
