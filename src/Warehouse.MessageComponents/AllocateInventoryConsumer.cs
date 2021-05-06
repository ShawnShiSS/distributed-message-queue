using MassTransit;
using System.Threading.Tasks;
using Warehouse.MessageContracts;

namespace Warehouse.MessageComponents
{
    public class AllocateInventoryConsumer
        : IConsumer<IAllocateInventory>
    {
        public async Task Consume(ConsumeContext<IAllocateInventory> context)
        {
            // Fake system-specific allocation workflow.
            await Task.Delay(500);

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
