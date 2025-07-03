using Application.Utilities.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.OrderItems.Commands.UpdateOrderItem
{
    public record UpdateOrderItemCommandResult : BaseCommandResult
    {
        public Guid OrderItemId { get; set; }
    }
    public record UpdateOrderItemCommand : IRequest<UpdateOrderItemCommandResult>
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    public class UpdateOrderItemCommandHandler : IRequestHandler<UpdateOrderItemCommand, UpdateOrderItemCommandResult>
    {
        private readonly ApplicationDbContext _context;

        public UpdateOrderItemCommandHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<UpdateOrderItemCommandResult> Handle(UpdateOrderItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
                if (order == null)
                {
                    return new UpdateOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.NotFound,
                        Errors = { "Order not found" }
                    };
                }
                if (order.Status > Domain.Common.OrderStatus.Preparing)
                {
                    return new UpdateOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.InvalidStatus,
                        Errors = { "Cannot add items to an order that is already completed or cancelled" }
                    };
                }
                if (request.Quantity <= 0)
                {
                    return new UpdateOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.InvalidQTY,
                        Errors = { "Quantity must be bigger than zero" }
                    };
                }
                if (request.UnitPrice <= 0)
                {
                    return new UpdateOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.InvalidPrice,
                        Errors = { "Unit price must be bigger than zero" }
                    };
                }
                var orderItem =await _context.OrderItems.FirstOrDefaultAsync(oi => oi.Id == request.Id , cancellationToken);
                if (orderItem == null)
                {
                    return new UpdateOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.NotFound,
                        Errors = { "Order item not found" }
                    };
                }
                if (orderItem.OrderId != request.OrderId)
                {
                    return new UpdateOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.InvalidOperation,
                        Errors = { "Order item does not belong to the specified order" }
                    };
                }
                orderItem.Name = request.Name;
                orderItem.Quantity = request.Quantity;
                orderItem.UnitPrice = request.UnitPrice;

                _context.OrderItems.Update(orderItem);
                await _context.SaveChangesAsync(cancellationToken);
                return new UpdateOrderItemCommandResult
                {
                    IsSuccess = true,
                    OrderItemId = orderItem.Id
                };
            }
            catch (Exception ex)
            {
                return new UpdateOrderItemCommandResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
