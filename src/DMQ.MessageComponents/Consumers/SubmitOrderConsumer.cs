using DMQ.MessageContracts;
using MassTransit;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.Consumers
{
    public class SubmitOrderConsumer : IConsumer<ISubmitOrder>
    {
        public async Task Consume(ConsumeContext<ISubmitOrder> context)
        {
            await context.RespondAsync<IOrderSubmissionAccepted>(new
            {
                OrderId = context.Message.OrderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = context.Message.CustomerNumber
            });
        }
    }
}
