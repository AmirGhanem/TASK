using Application.Services.UserService;
using Application.Utilities.Models;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Orders.Commands.ChangeOrderStatus
{
    public record ChangeOrderStatusCommandResult : BaseCommandResult
    {
        public Guid OrderId { get; set; }
    }
    public record ChangeOrderStatusCommand : IRequest<ChangeOrderStatusCommandResult>
    {
        public Guid Id { get; set; }
        public OrderStatus Status { get; set; }
    }

    public class ChangeOrderStatusCommandHandler : IRequestHandler<ChangeOrderStatusCommand, ChangeOrderStatusCommandResult>
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public ChangeOrderStatusCommandHandler(ApplicationDbContext context,IUserService userService)
        {
            this._context = context;
            this._userService = userService;
        }
        public async Task<ChangeOrderStatusCommandResult> Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.Id);
                if (order == null)
                {
                    return new ChangeOrderStatusCommandResult
                    {
                        IsSuccess = false,
                        Errors = { "Order not found" },
                        ErrorCode = Domain.Common.ErrorCode.NotFound
                    };
                }
                order.Status = request.Status;
                order.StatusChangedById = user.Id;
                order.StatusChangeDate = DateTime.UtcNow.Date;
                
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                return new ChangeOrderStatusCommandResult
                {
                    IsSuccess = true,
                    OrderId = order.Id
                };
            }
            catch (Exception ex)
            {
                return new ChangeOrderStatusCommandResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
