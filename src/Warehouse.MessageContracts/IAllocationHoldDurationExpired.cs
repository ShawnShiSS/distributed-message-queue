using System;

namespace Warehouse.MessageContracts
{
    public interface IAllocationHoldDurationExpired
    { 
        Guid AllocationId { get; }
    }
}
