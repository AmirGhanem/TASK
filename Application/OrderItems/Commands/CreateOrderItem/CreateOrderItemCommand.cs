using Application.Utilities.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.OrderItems.Commands.CreateOrderItem
{
    public record CreateOrderItemCommandResult : BaseCommandResult
    {
        public Guid OrderItemId { get; set; }
    }
    public record CreateOrderItemCommand: IRequest<CreateOrderItemCommandResult>
    {
        public Guid OrderId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    public class CreateOrderItemCommandHandler : IRequestHandler<CreateOrderItemCommand, CreateOrderItemCommandResult>
    {
        private readonly ApplicationDbContext _context;

        public CreateOrderItemCommandHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<CreateOrderItemCommandResult> Handle(CreateOrderItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
                if (order == null)
                {
                    return new CreateOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.NotFound,
                        Errors = { "Order not found" }
                    };
                }
                if(order.Status > Domain.Common.OrderStatus.Preparing)
                {
                    return new CreateOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.InvalidStatus,
                        Errors = { "Cannot add items to an order that is already completed or cancelled" }
                    };
                }
                if (request.Quantity <= 0)
                {
                    return new CreateOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.InvalidQTY,
                        Errors = { "Quantity must be bigger than zero" }
                    };
                }
                if (request.UnitPrice <= 0)
                {
                    return new CreateOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.InvalidPrice,
                        Errors = { "Unit price must be bigger than zero" }
                    };
                }
                var orderItem = new Domain.Models.OrderItems.OrderItem
                {
                    OrderId = request.OrderId,
                    Name = request.Name,
                    Quantity = request.Quantity,
                    UnitPrice = request.UnitPrice
                };
                await _context.OrderItems.AddAsync(orderItem, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return new CreateOrderItemCommandResult
                {
                    IsSuccess = true,
                    OrderItemId = orderItem.Id
                };
            }
            catch (Exception ex)
            {
                return new CreateOrderItemCommandResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
