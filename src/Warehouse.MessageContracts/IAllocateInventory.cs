using System;

namespace Warehouse.MessageContracts
{
    /// <summary>
    ///     Allocate an item in the warehouse so it can be picked up.
    /// </summary>
    public interface IAllocateInventory
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
