using Application.Orders.Commands.ChangeOrderStatus;
using Application.Orders.Commands.CreateOrder;
using Application.Orders.Commands.DeleteOrder;
using Application.Orders.Commands.UpdateOrder;
using Application.Orders.Queries.GetOrder;
using Application.Orders.Queries.GetOrders;
using Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Orders
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        [HttpPost("Create")]
        public async Task<ActionResult<CreateOrderCommandResult>> CreateOrder([FromBody] CreateOrderCommand command)
        {
            return await this.HandleCommandResult(_mediator.Send(command));
        }

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        [HttpPut("Update")]
        public async Task<ActionResult<UpdateOrderCommandResult>> UpdateOrder([FromBody] UpdateOrderCommand command)
        {
            return await this.HandleCommandResult(_mediator.Send(command));
        }

        /// <summary>
        /// Deletes an order.
        /// </summary>
        [HttpDelete("Delete")]
        public async Task<ActionResult<DeleteOrderCommandResult>> DeleteOrder([FromBody] DeleteOrderCommand command)
        {
            return await this.HandleCommandResult(_mediator.Send(command));
        }

        /// <summary>
        /// Changes the status of an order.
        /// </summary>
        [HttpPatch("ChangeStatus")]
        public async Task<ActionResult<ChangeOrderStatusCommandResult>> ChangeOrderStatus([FromBody] ChangeOrderStatusCommand command)
        {
            return await this.HandleCommandResult(_mediator.Send(command));
        }

        /// <summary>
        /// Retrieves a single order by ID.
        /// </summary>
        [HttpGet("Get")]
        public async Task<ActionResult<GetOrderQueryResult>> GetOrder([FromQuery] GetOrderQuery query)
        {
            return await this.HandleCommandResult(_mediator.Send(query));
        }

        /// <summary>
        /// Retrieves a list of orders.
        /// </summary>
        [HttpPost("GetAll")]
        public async Task<ActionResult<GetOrdersQueryResult>> GetOrders([FromBody] GetOrdersQuery query)
        {
            return await this.HandleCommandResult(_mediator.Send(query));
        }
    }
}
