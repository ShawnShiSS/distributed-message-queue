﻿using DMQ.MessageContracts;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Courier.Contracts;
using System;
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
            // One for execution, one for compensate.
            // By convention, AllocateInventory will have a queue name of kebab-case: allocate-inventory_execute
            builder.AddActivity("AllocateInventory", 
                                new Uri("queue:allocate-inventory_execute"),
                                new 
                                {
                                    ItemNumber = "ITEM123",
                                    Quantity = 10.0m
                                });

            builder.AddActivity("PaymentActivity",
                                new Uri("queue:payment_execute"),
                                new 
                                {
                                    CardNumber = "4000", // Fake card number for now
                                    Amount = 99.99m
                                });

            // All activities can access an variable, so that we do not repeating it multiple times in the arguments above throughout the routing
            builder.AddVariable("OrderId", context.Message.OrderId);

            // Using source address since we know where the caller is from
            await builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.Faulted | RoutingSlipEvents.Supplemental, RoutingSlipEventContents.None, x => x.Send<IOrderFulfillmentFaulted>(new { context.Message.OrderId}));

            await builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.Completed| RoutingSlipEvents.Supplemental, RoutingSlipEventContents.None, x => x.Send<IOrderFulfillmentCompleted>(new { context.Message.OrderId }));

            // Create the routing slip
            var routingSlip = builder.Build();

            // Trigger the execution of the routing slip starting on the first activity in the iternary
            await context.Execute(routingSlip);
        }
    }


}
