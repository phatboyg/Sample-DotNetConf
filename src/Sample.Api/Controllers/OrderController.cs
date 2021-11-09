namespace Sample.Api.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;


    [ApiController]
    [Route("[controller]")]
    public class OrderController :
        ControllerBase
    {
        readonly ILogger<OrderController> _logger;
        readonly IPublishEndpoint _publishEndpoint;
        readonly IRequestClient<AcceptOrder> _acceptOrderClient;
        readonly IRequestClient<GetOrder> _getOrderClient;

        public OrderController(ILogger<OrderController> logger, IPublishEndpoint publishEndpoint, IRequestClient<AcceptOrder> acceptOrderClient,
            IRequestClient<GetOrder> getOrderClient)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _acceptOrderClient = acceptOrderClient;
            _getOrderClient = getOrderClient;
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid orderId, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Response response = await _getOrderClient.GetResponse<Order, OrderNotFound>(new
            {
                orderId,
            }, cancellationToken);

            return response switch
            {
                (_, Order x) => Ok(new OrderModel
                {
                    OrderId = x.OrderId,
                    OrderNumber = x.OrderNumber,
                    Status = x.Status
                }),
                (_, OrderNotFound x) => NotFound(new OrderModel
                {
                    OrderId = x.OrderId,
                }),
                _ => BadRequest(new OrderModel
                {
                    OrderId = orderId,
                })
            };
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Submit([FromBody] SubmitOrderModel orderModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _publishEndpoint.Publish<SubmitOrder>(new
            {
                orderModel.OrderId,
                orderModel.OrderNumber
            }, cancellationToken);

            return Accepted(new OrderModel
            {
                OrderId = orderModel.OrderId,
                OrderNumber = orderModel.OrderNumber,
                Status = "Submitted"
            });
        }

        [HttpPost("{orderId}/accept")]
        [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Accept(Guid orderId, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Response response = await _acceptOrderClient.GetResponse<Order, OrderNotFound>(new
            {
                orderId,
            }, cancellationToken);

            return response switch
            {
                (_, Order x) => Ok(new OrderModel
                {
                    OrderId = x.OrderId,
                    OrderNumber = x.OrderNumber,
                    Status = x.Status
                }),
                (_, OrderNotFound x) => NotFound(new OrderModel
                {
                    OrderId = x.OrderId,
                }),
                _ => BadRequest(new OrderModel
                {
                    OrderId = orderId,
                })
            };
        }
    }
}