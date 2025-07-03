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

namespace Application.Orders.Commands.CreateOrder
{
    public record CreateOrderCommandResult : BaseCommandResult
    {
        public Guid OrderId { get; set; }
    }
    public record CreateOrderCommand : IRequest<CreateOrderCommandResult>
    {
        public string CustomerName { get; set; }
        public List<OrderItemVM> Items { get; set; } = new List<OrderItemVM>();
    }
    public record OrderItemVM
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }
    }
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderCommandResult>
    {
        private readonly ApplicationDbContext _context;

        public CreateOrderCommandHandler(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task<CreateOrderCommandResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Items.Any(i => i.Quantity <= 0))
                {
                    return new CreateOrderCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.InvalidQTY,
                        Errors = { "Quantity must be bigger Than zero" }
                    };
                }
                if (request.Items.Any(i => i.UnitPrice <= 0))
                {
                    return new CreateOrderCommandResult
                    {
                        IsSuccess = false,
                        ErrorCode = Domain.Common.ErrorCode.InvalidPrice,
                        Errors = { "Price must be bigger Than zero" }
                    };
                }
                    var order = new Order
                    {
                        CustomerName = request.CustomerName,
                        Status = Domain.Common.OrderStatus.Pending,
                        OrderItems = request.Items.Select(x => new OrderItem
                        {
                            Quantity = x.Quantity,
                            UnitPrice = x.UnitPrice,
                            Name = x.Name,
                        }).ToList()
                    };
                    await _context.Orders.AddAsync(order);
                    await _context.SaveChangesAsync();

                    return new CreateOrderCommandResult
                    {
                        IsSuccess = true,
                        OrderId = order.Id
                    };
                }
            catch (Exception ex)
            {
                return new CreateOrderCommandResult
                {
                    IsSuccess = false,
                    ErrorCode = Domain.Common.ErrorCode.Error,
                    Errors = { ex.Message }
                };
            }
        }
    }
}
