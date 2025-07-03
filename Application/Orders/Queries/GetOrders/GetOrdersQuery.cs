using Application.Orders.Queries.GetOrder;
using Application.Utilities.Extensions;
using Application.Utilities.Filter;
using Application.Utilities.Models;
using Application.Utilities.Sort;
using Domain.Common;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Orders.Queries.GetOrders
{
    public record GetOrdersQueryResult : BaseCommandResult
    {
        public BasePaginatedList<OrderDetailsVM> Orders { get; set; }
    }
    public record GetOrdersQuery :BasePaginatedQuery, IRequest<GetOrdersQueryResult>
    {
    }
   
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersQueryResult>
    {
        private readonly ApplicationDbContext _context;

        public GetOrdersQueryHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<GetOrdersQueryResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.StatusChangedBy)
                    .Search(request.SearchTerm)
                    .Select(o => new OrderDetailsVM
                    {
                        CustomerName = o.CustomerName,
                        Id = o.Id,
                        Items = o.OrderItems.Select(i => new OrderItemDetailsVM
                        {
                            Id = i.Id,
                            Name = i.Name,
                            OrderId = i.OrderId,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            CreatedDate = o.CreatedDate,
                            ModifiedDate = o.ModifiedDate
                        }).ToList(),
                        No = o.No,
                        Status = o.Status,
                        StatusChangedById = o.StatusChangedById,
                        StatusChangedBy = o.StatusChangedBy != null ? o.StatusChangedBy.Name : null,
                        StatusChangeDate = o.StatusChangeDate,
                        CreatedDate = o.CreatedDate,
                        ModifiedDate = o.ModifiedDate

                    })
                    .Filter(request.Filters)
                    .Sort(request.Sorts)
                    .ToPaginatedListAsync(request.PageNumber, request.PageSize);



                return new GetOrdersQueryResult
                {
                    IsSuccess = true,
                    Orders = orders
                };
            }
            catch (Exception ex)
            {
                return new GetOrdersQueryResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
