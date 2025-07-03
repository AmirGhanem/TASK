using Application.Orders.Queries.GetOrder;
using Application.Utilities.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.OrderItems.Queries.GetOrderItem
{
    public record GetOrderItemQueryResult : BaseCommandResult
    {
        public OrderItemDetailsVM OrderItem { get; set; }
    }
    public record GetOrderItemQuery : IRequest<GetOrderItemQueryResult>
    {
        public Guid Id { get; set; }

    }
  
    public class GetOrderItemQueryHandler : IRequestHandler<GetOrderItemQuery, GetOrderItemQueryResult>
    {
        private readonly ApplicationDbContext _context;

        public GetOrderItemQueryHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<GetOrderItemQueryResult> Handle(GetOrderItemQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var orderItem = await _context.OrderItems
                    .Select(oi => new OrderItemDetailsVM
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        Name = oi.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        CreatedDate = oi.CreatedDate,
                        ModifiedDate = oi.ModifiedDate
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync(oi => oi.Id == request.Id, cancellationToken);
                if (orderItem == null)
                {
                    return new GetOrderItemQueryResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.NotFound,
                        Errors = { "Order item not found" }
                    };
                }
               

              
                return new GetOrderItemQueryResult
                {
                    IsSuccess = true,
                    OrderItem = orderItem
                };
            }
            catch (Exception ex)
            {
                return new GetOrderItemQueryResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
