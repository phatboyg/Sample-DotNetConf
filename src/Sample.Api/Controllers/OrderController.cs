namespace Sample.Api.Controllers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;


    [ApiController]
    [Route("[controller]")]
    public class OrderController :
        ControllerBase
    {
        readonly OrderDbContext _dbContext;
        readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger, OrderDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid orderId, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await _dbContext.Orders.Where(x => x.OrderId == orderId).SingleOrDefaultAsync(cancellationToken);
            if (order is null)
                return NotFound(new { OrderId = orderId });

            return Ok(new OrderModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                Status = order.Status.ToString()
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Submit([FromBody] SubmitOrderModel orderModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = new Order
            {
                OrderId = orderModel.OrderId,
                OrderNumber = orderModel.OrderNumber,
                Status = OrderStatus.Submitted
            };

            await _dbContext.AddAsync(order, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new OrderModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                Status = order.Status.ToString()
            });
        }

        [HttpPost("{orderId}/accept")]
        [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Accept(Guid orderId, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await _dbContext.Orders.Where(x => x.OrderId == orderId).SingleOrDefaultAsync(cancellationToken);
            if (order is null)
                return NotFound(new { OrderId = orderId });

            order.Status = OrderStatus.Accepted;

            _dbContext.Update(order);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new OrderModel
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                Status = order.Status.ToString()
            });
        }
    }
}