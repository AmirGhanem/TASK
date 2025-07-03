using Application.Utilities.Models;
using Domain.Models.OrderItems;
using Domain.Models.Orders;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Orders.Commands.UpdateOrder
{
    public record UpdateOrderCommandResult : BaseCommandResult
    {
        public Guid OrderId { get; set; }
    }
    public record UpdateOrderCommand : IRequest<UpdateOrderCommandResult>
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
    }

    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, UpdateOrderCommandResult>
    {
        private readonly ApplicationDbContext _context;

        public UpdateOrderCommandHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<UpdateOrderCommandResult> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.Id);
                if (order == null)
                {
                    return new UpdateOrderCommandResult
                    {
                        IsSuccess = false,
                        Errors = { "Order not found" },
                        ErrorCode = Domain.Common.ErrorCode.NotFound
                    };
                }
                order.CustomerName = request.CustomerName;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                return new UpdateOrderCommandResult
                {
                    IsSuccess = true,
                    OrderId = order.Id
                };
            }
            catch (Exception ex)
            {
                return new UpdateOrderCommandResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
