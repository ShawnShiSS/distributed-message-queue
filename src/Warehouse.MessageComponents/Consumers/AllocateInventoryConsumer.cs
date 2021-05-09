using MassTransit;
using System.Threading.Tasks;
using Warehouse.MessageContracts;

namespace Warehouse.MessageComponents.Consumers
{
    public class AllocateInventoryConsumer
        : IConsumer<IAllocateInventory>
    {
        public async Task Consume(ConsumeContext<IAllocateInventory> context)
        {
            // Fake system-specific allocation workflow.
            await Task.Delay(500);

            // Publish an event
            await context.Publish<IAllocationCreated>(new
            {
                AllocationId = context.Message.AllocationId,
                // Time span in miliseconds
                HoldDuration = 6000
            });

            // Return response.
            await context.RespondAsync<IInventoryAllocated>(new
            {
                AllocationId = context.Message.AllocationId,
                ItemNumber = context.Message.ItemNumber,
                Quantity = context.Message.Quantity
            });
        }
    }
}
