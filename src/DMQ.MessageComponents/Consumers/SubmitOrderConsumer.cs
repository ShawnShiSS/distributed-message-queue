using DMQ.MessageContracts;
using MassTransit;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.Consumers
{
    public class SubmitOrderConsumer : IConsumer<ISubmitOrder>
    {
        public Task Consume(ConsumeContext<ISubmitOrder> context)
        {
            throw new System.NotImplementedException();
        }
    }
}
