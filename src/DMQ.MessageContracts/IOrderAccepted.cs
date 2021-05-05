using System;

namespace DMQ.MessageContracts
{
    public interface IOrderAccepted
    {
        Guid OrderId { get; }
    }

}
