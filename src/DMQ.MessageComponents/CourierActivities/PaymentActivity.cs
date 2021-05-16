using MassTransit.Courier;
using System;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.CourierActivities
{
    public class PaymentActivity :
        IActivity<IPaymentArguments, IPaymentLog>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<IPaymentArguments> context)
        {
            string cardNumber = context.Arguments.CardNumber;

            if (string.IsNullOrEmpty(cardNumber))
            {
                throw new ArgumentNullException(nameof(cardNumber));
            }

            // Process payment
            // Fake delay, allowing the allocation to get into allocated state and take hold in the other Saga (See AllocateInventoryConsumer.cs).
            await Task.Delay(2000);

            if (cardNumber.StartsWith("5999"))
            {
                throw new InvalidCastException("Card number is not valid");
            }

            return context.Completed(new { AuthorizedCode = "88888" });
        }

        public async Task<CompensationResult> Compensate(CompensateContext<IPaymentLog> context)
        {
            // Compensate payment 

            await Task.Delay(300);

            return context.Compensated();
        }
    }
}
