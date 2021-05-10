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
        private readonly IRequestClient<ICheckOrder> _checkOrderRequestClient;
        /// <summary>
        ///     Send endpoint provider, which is part of the Mass Transit IBus interface, which manages send/publish endpoints.
        ///     Startup.cs/AddMassTransit() registers all the endpoints, and the consumers will resolve them.
        /// </summary>
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderController(ILogger<OrderController> logger,
                               IRequestClient<ISubmitOrder> submitOrderRequestClient,
                               IRequestClient<ICheckOrder> checkOrderRequestClient,
                               ISendEndpointProvider sendEndpointProvider,
                               IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _submitOrderRequestClient = submitOrderRequestClient;
            _checkOrderRequestClient = checkOrderRequestClient;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        // Database for the order state is owned by another micro-service, so the API can not just talks to the database directly.
        // GET: api/Order/5
        /// <summary>
        ///     Get order status
        /// </summary>
        /// <param name="id">Order id</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetOrderStatus")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<IActionResult> GetOrderStatus(Guid id)
        {
            var (status, notFound) = await _checkOrderRequestClient.GetResponse<IOrderStatus, IOrderNotFound>(new
            {
                OrderId = id
            });

            if (status.IsCompletedSuccessfully)
            {
                var response = await status;
                return Ok(response.Message);
            }

            var notFoundResponse = await notFound;
            return NotFound(notFoundResponse.Message);
        }

        // Publish a message without specifying a queue.
        // Post: api/Order
        /// <summary>
        ///     Create a new order
        /// </summary>
        /// <param name="id">Order Id</param>
        /// <param name="customerNumber"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Create))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Create([FromBody] DMQ.API.Models.Order.Create createCommand)
        {
            // Tuple response from the consumer
            var (accepted, rejected) = await _submitOrderRequestClient.GetResponse<IOrderSubmissionAccepted, IOrderSubmissionRejected>(new
            {
                OrderId = createCommand.OrderId,
                Timestamp = InVar.Timestamp,
                CustomerNumber = createCommand.CustomerNumber
            });

            if (accepted.IsCompletedSuccessfully)
            {
                return Accepted(await accepted);
            }

            return BadRequest(await rejected);
        }

        
        // Demo how to send a message directly to a queue, kind of cheating.
        [HttpPut("{id}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
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
        

        // PUT: api/Order/{id}/Acceptance
        /// <summary>
        ///     Accept an order
        /// </summary>
        /// <param name="id">Order id.</param>
        /// <returns></returns>
        [HttpPut("{id}/Acceptance", Name ="AcceptOrder")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Update))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> AcceptOrder(Guid id)
        {
            await _publishEndpoint.Publish<IOrderAccepted>(new
            {
                OrderId = id
            });

            return Accepted();
        }
    }
}
