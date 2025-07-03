using Application.Orders.Queries.GetOrder;
using Application.Utilities.Extensions;
using Application.Utilities.Filter;
using Application.Utilities.Models;
using Application.Utilities.Sort;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.OrderItems.Queries.GetOrderItems
{
    public record GetOrderItemsQueryResult : BaseCommandResult
    {
        public BasePaginatedList<OrderItemDetailsVM> OrderItems { get; set; }
    }
    public record GetOrderItemsQuery : BasePaginatedQuery, IRequest<GetOrderItemsQueryResult>
    {
        public Guid OrderId { get; set; }

    }

    public class GetOrderItemsQueryHandler : IRequestHandler<GetOrderItemsQuery, GetOrderItemsQueryResult>
    {
        private readonly ApplicationDbContext _context;

        public GetOrderItemsQueryHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<GetOrderItemsQueryResult> Handle(GetOrderItemsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == request.OrderId)
                    .Search(request.SearchTerm)
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
                    .Filter(request.Filters)
                    .Sort(request.Sorts)
                    .ToPaginatedListAsync(request.PageNumber, request.PageSize);
               


                return new GetOrderItemsQueryResult
                {
                    IsSuccess = true,
                    OrderItems = orderItems
                };
            }
            catch (Exception ex)
            {
                return new GetOrderItemsQueryResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
