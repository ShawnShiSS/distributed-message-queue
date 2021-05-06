using MassTransit;
using MassTransit.Courier;
using System;
using System.Threading.Tasks;
using Warehouse.MessageContracts;

namespace DMQ.MessageComponents.CourierActivities
{
    /// <summary>
    ///     Activity that will communicate with remote server and bring back acknowledgement that inventory has been allocated.
    /// </summary>
    public class AllocateInventoryActivity
        : IActivity<IAllocateInventoryArguments, IAllocateInventoryLog>
    {
        // Note: IAllocateInventory message contract can be from a NuGet package, or directly referenced like how it is done here.

        private readonly IRequestClient<IAllocateInventory> _requestClient;

        public AllocateInventoryActivity(IRequestClient<IAllocateInventory> requestClient)
        {
            this._requestClient = requestClient;
        }

        /// <summary>
        ///     The actual work that calls out to the remote server to allocate.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<ExecutionResult> Execute(ExecuteContext<IAllocateInventoryArguments> context)
        {
            var orderId = context.Arguments.OrderId;
            var itemNumber = context.Arguments.ItemNumber;
            var quantity = context.Arguments.Quantity;

            if (string.IsNullOrEmpty(itemNumber))
            {
                throw new ArgumentNullException(nameof(itemNumber));
            }
            
            if (quantity <= 0)
            {
                throw new ArgumentNullException(nameof(quantity));
            }

            Guid allocationId = Guid.NewGuid();

            // No need to try-catch the request below.
            // Let runtime fault the activity if it fails.
            var response = await _requestClient.GetResponse<IInventoryAllocated>(new
            {
                AllocationId = allocationId,
                ItemNumber = itemNumber,
                Quantity = quantity
            });

            // Now that I've allocated the item in the warehouse waiting for someone to pick it up, let's log it.
            return context.Completed(new
            {
                AllocationId = allocationId
            });

        }

        /// <summary>
        ///     For compensating a routing slip.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<CompensationResult> Compensate(CompensateContext<IAllocateInventoryLog> context)
        {
            // Let's publish a request to release the allocation.
            await context.Publish<IAllocationReleaseRequested>(new
            {
                AllocationId = context.Log.AllocationId,
                Reason = "Order faulted"
            });

            return context.Compensated();
        }
    }
}
