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

            if (cardNumber.StartsWith("5999"))
            {
                throw new InvalidCastException("Card number is not valid");
            }

            // Process payment
            await Task.Delay(300);

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
