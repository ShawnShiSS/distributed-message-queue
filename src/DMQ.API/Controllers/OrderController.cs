using DMQ.MessageContracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMQ.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
       
        private readonly ILogger<OrderController> _logger;
        private readonly IRequestClient<ISubmitOrder> _submitOrderRequestClient;
        /// <summary>
        ///     Send endpoint provider, which is part of the Mass Transit IBus interface, which manages send/publish endpoints.
        ///     Startup.cs/AddMassTransit() registers all the endpoints, and the consumers will resolve them.
        /// </summary>
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrderController(ILogger<OrderController> logger,
                               IRequestClient<ISubmitOrder> submitOrderRequestClient,
                               ISendEndpointProvider sendEndpointProvider)
        {
            _logger = logger;
            _submitOrderRequestClient = submitOrderRequestClient;
            _sendEndpointProvider = sendEndpointProvider;
        }

        // Publish a message without specifying a queue.
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(Guid id, string customerNumber)
        {
            // Tuple response from the consumer
            var (accepted, rejected) = await _submitOrderRequestClient.GetResponse<IOrderSubmissionAccepted, IOrderSubmissionRejected>(new
            {
                OrderId = id,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });

            if (accepted.IsCompletedSuccessfully)
            {
                return Accepted(await accepted);
            }

            return BadRequest(await rejected);
        }

        // Send a message directly to a queue
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(Guid id, string customerNumber)
        {
            // Get the queue endpoint and send message to it.
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:submit-order"));
            await endpoint.Send<ISubmitOrder>(new 
            {
                OrderId = id,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });

            return Accepted();
        }
    }
}
