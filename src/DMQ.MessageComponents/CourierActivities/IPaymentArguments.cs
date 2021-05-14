using System;

namespace DMQ.MessageComponents.CourierActivities
{
    public interface IPaymentArguments
    {
        Guid OrderId { get; set; }
        decimal Amount { get; set; }
        string CardNumber { get; set; }
    }
}
