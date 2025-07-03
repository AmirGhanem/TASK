using Application.Utilities.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Orders.Commands.DeleteOrder
{
    public record DeleteOrderCommandResult : BaseCommandResult
    {
    }
    public record DeleteOrderCommand : IRequest<DeleteOrderCommandResult>
    {
        public Guid Id { get; set; }
    }

    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, DeleteOrderCommandResult>
    {
        private readonly ApplicationDbContext _context;

        public DeleteOrderCommandHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<DeleteOrderCommandResult> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == request.Id);
                if (order == null)
                {
                    return new DeleteOrderCommandResult
                    {
                        IsSuccess = false,
                        Errors = { "Order not found" },
                        ErrorCode = Domain.Common.ErrorCode.NotFound
                    };
                }
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                return new DeleteOrderCommandResult
                {
                    IsSuccess = true,
                };
            }
            catch (Exception ex)
            {
                return new DeleteOrderCommandResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
