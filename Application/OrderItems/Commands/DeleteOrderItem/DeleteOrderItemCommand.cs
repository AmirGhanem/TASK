using Application.Utilities.Models;
using Domain.Models.Orders;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.OrderItems.Commands.DeleteOrderItem
{
    public record DeleteOrderItemCommandResult : BaseCommandResult
    {
    }
    public record DeleteOrderItemCommand : IRequest<DeleteOrderItemCommandResult>
    {
        public Guid Id { get; set; }
       
    }
    public class DeleteOrderItemCommandHandler : IRequestHandler<DeleteOrderItemCommand, DeleteOrderItemCommandResult>
    {
        private readonly ApplicationDbContext _context;

        public DeleteOrderItemCommandHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<DeleteOrderItemCommandResult> Handle(DeleteOrderItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orderItem = await _context.OrderItems
                    .Include(i => i.Order)
                    .FirstOrDefaultAsync(oi => oi.Id == request.Id, cancellationToken);
                if (orderItem == null)
                {
                    return new DeleteOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.NotFound,
                        Errors = { "Order item not found" }
                    };
                }
                if (orderItem.Order.Status > Domain.Common.OrderStatus.Preparing)
                {
                    return new DeleteOrderItemCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.InvalidStatus,
                        Errors = { "Cannot add items to an order that is already completed or cancelled" }
                    };
                }
               

                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync(cancellationToken);
                return new DeleteOrderItemCommandResult
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new DeleteOrderItemCommandResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
