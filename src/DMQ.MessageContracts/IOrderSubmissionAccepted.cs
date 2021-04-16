using System;

namespace DMQ.MessageContracts
{
    public interface IOrderSubmissionAccepted
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
        string CustomerNumber { get; }

    }
}
