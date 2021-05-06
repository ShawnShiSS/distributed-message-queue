using System;

namespace Warehouse.MessageContracts
{
    /// <summary>
    ///     Request to release the allocation and provide a reason.
    /// </summary>
    public interface IAllocationReleaseRequested
    {
        Guid AllocationId { get; }
        string Reason { get; }
    }
}
