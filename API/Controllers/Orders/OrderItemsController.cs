using Application.OrderItems.Commands.CreateOrderItem;
using Application.OrderItems.Commands.DeleteOrderItem;
using Application.OrderItems.Commands.UpdateOrderItem;
using Application.OrderItems.Queries.GetOrderItem;
using Application.OrderItems.Queries.GetOrderItems;
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
    public class OrderItemsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new order item.
        /// </summary>
        [HttpPost("Create")]
        public async Task<ActionResult<CreateOrderItemCommandResult>> CreateOrderItem([FromBody] CreateOrderItemCommand command)
        {
            return await this.HandleCommandResult(_mediator.Send(command));
        }

        /// <summary>
        /// Updates an existing order item.
        /// </summary>
        [HttpPut("Update")]
        public async Task<ActionResult<UpdateOrderItemCommandResult>> UpdateOrderItem([FromBody] UpdateOrderItemCommand command)
        {
            return await this.HandleCommandResult(_mediator.Send(command));
        }

        /// <summary>
        /// Deletes an order item.
        /// </summary>
        [HttpDelete("Delete")]
        public async Task<ActionResult<DeleteOrderItemCommandResult>> DeleteOrderItem([FromBody] DeleteOrderItemCommand command)
        {
            return await this.HandleCommandResult(_mediator.Send(command));
        }

        /// <summary>
        /// Retrieves a single order item by ID.
        /// </summary>
        [HttpGet("Get")]
        public async Task<ActionResult<GetOrderItemQueryResult>> GetOrderItem([FromQuery] GetOrderItemQuery query)
        {
            return await this.HandleCommandResult(_mediator.Send(query));
        }

        /// <summary>
        /// Retrieves a list of order items.
        /// </summary>
        [HttpPost("GetAll")]
        public async Task<ActionResult<GetOrderItemsQueryResult>> GetOrderItems([FromBody] GetOrderItemsQuery query)
        {
            return await this.HandleCommandResult(_mediator.Send(query));
        }
    }
}
