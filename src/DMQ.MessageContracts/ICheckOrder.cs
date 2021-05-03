using System;
using System.Collections.Generic;
using System.Text;

namespace DMQ.MessageContracts
{
    public interface ICheckOrder
    {
        Guid OrderId { get; }
    }

}
