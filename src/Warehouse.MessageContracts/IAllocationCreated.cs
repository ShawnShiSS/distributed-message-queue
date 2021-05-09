using System;

namespace Warehouse.MessageContracts
{
    public interface IAllocationCreated
    {
        Guid AllocationId { get; }
        /// <summary>
        ///     How long the allocation should be held for.
        /// </summary>
        TimeSpan HoldDuration { get; }
    }
}
