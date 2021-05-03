using System;

namespace DMQ.MessageContracts
{
    public interface IOrderNotFound
    { 
        Guid OrderId { get; }
    }

}
