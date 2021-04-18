using DMQ.MessageContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.Consumers
{
    /// <summary>
    ///     Consumer for ISubmitOrder message.
    ///     Note: all consumers are registered as scoped with the DI container.
    /// </summary>
    public class SubmitOrderConsumer : IConsumer<ISubmitOrder>
    {
        private readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ISubmitOrder> context)
        {
            _logger.LogInformation($"SubmitOrderConsumer for customer number: {context.Message.CustomerNumber}");

            // Only respond if a response is expected
            if (context.ResponseAddress == null)
            {
                return;
            }

            // Generate response
            if (!IsValidOrder(context.Message.CustomerNumber))
            {
                await context.RespondAsync<IOrderSubmissionRejected>(new 
                {
                    OrderId = context.Message.OrderId,
                    Timestamp = InVar.Timestamp,
                    CustomerNumber = context.Message.CustomerNumber,
                    Reason = $"Customer can not be Test: {context.Message.CustomerNumber}"
                });

                return;
            }

            await context.RespondAsync<IOrderSubmissionAccepted>(new
            {
                OrderId = context.Message.OrderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = context.Message.CustomerNumber
            });
        }

        private bool IsValidOrder(string customerNumber)
        {
            return !customerNumber.Contains("TEST");
        }
    }
}
