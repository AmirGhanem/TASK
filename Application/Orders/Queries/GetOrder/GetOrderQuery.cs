using Application.Utilities.Models;
using Domain.Common;
using Domain.Identity;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Orders.Queries.GetOrder
{
    public record GetOrderQueryResult : BaseCommandResult
    {
        public OrderDetailsVM Order { get; set; }
    }
    public record GetOrderQuery : IRequest<GetOrderQueryResult>
    {
        public Guid Id { get; set; }
    }
    public record OrderDetailsVM
    {
        public Guid Id { get; set; }
        public int No { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public OrderStatus Status { get; set; }
        public string? StatusChangedById { get; set; }
        public string? StatusChangedBy { get; set; }
        public DateTime? StatusChangeDate { get; set; }
        public List<OrderItemDetailsVM> Items { get; set; } = new List<OrderItemDetailsVM>();
        public decimal TotalAmount
        {
            get
            {
                return Items.Sum(i => i.TotalPrice);
            }
        }
    }
    public record OrderItemDetailsVM
    {
        public Guid Id { get; set; }
        public Guid OrderId { get;  set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice
        {
            get
            {
                return UnitPrice * Quantity;
            }
        }

        public DateTime CreatedDate { get;  set; }
        public DateTime ModifiedDate { get;  set; }
    }
    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, GetOrderQueryResult>
    {
        private readonly ApplicationDbContext _context;

        public GetOrderQueryHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<GetOrderQueryResult> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.StatusChangedBy)
                    .Select(o => new OrderDetailsVM
                    {
                        CustomerName = o.CustomerName,
                        Id = o.Id,
                        Items = o.OrderItems.Select(i => new OrderItemDetailsVM
                        {
                            Id = i.Id,
                            OrderId = i.OrderId,
                            Name = i.Name,
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
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == request.Id);
                if (order == null)
                {
                    return new GetOrderQueryResult
                    {
                        IsSuccess = false,
                        Errors = { "Order not found" },
                        ErrorCode = Domain.Common.ErrorCode.NotFound
                    };
                }


                return new GetOrderQueryResult
                {
                    IsSuccess = true,
                    Order = order
                };
            }
            catch (Exception ex)
            {
                return new GetOrderQueryResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
