using System;

namespace DMQ.MessageComponents.CourierActivities
{
    public interface IAllocateInventoryArguments
    {
        Guid OrderId { get; }
        string ItemNumber { get; }
        decimal Quantity { get; }

    }
}
