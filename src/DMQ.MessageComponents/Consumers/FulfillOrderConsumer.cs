using DMQ.MessageContracts;
using MassTransit;
using MassTransit.Courier;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DMQ.MessageComponents.Consumers
{
    public class FulfillOrderConsumer
        : IConsumer<IFulfillOrder>
    {
        public async Task Consume(ConsumeContext<IFulfillOrder> context)
        {
            // Build and execute the routing slip
            // Every routing slip has a tracking number, like a Fedex package
            var builder = new RoutingSlipBuilder(Guid.NewGuid());

            // For each endpoint hosting an activity,
            // two queues are required to execute one activity
            // One for execution, one for compensate
            builder.AddActivity("AllocateInventory", 
                                new Uri("queue:allocate-inventory_execute"),
                                new 
                                {
                                    ItemNumber = "ITEM123",
                                    Quantity = 10.0m
                                });
            // All activities can use an variable, so that we do not repeating it multiple times in the arguments above throughout the routing
            builder.AddVariable("OrderId", context.Message.OrderId);

            // Create the routing slip
            var routingSlip = builder.Build();

            // Trigger the execution of the routing slip starting on the first activity in the iternary
            await context.Execute(routingSlip);
        }
    }


}
