using System;

namespace DMQ.MessageComponents.CourierActivities
{
    public interface IAllocateInventoryLog
    {
        /// <summary>
        ///     Reference token used to de-allocate/rollback if anything fails.
        /// </summary>
         Guid AllocationId { get; }
    }
}
