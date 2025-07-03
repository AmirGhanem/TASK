using Domain.Models.Contractors;
using Domain.Models.Orders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.OrderItems
{
    public class OrderItem : ModelBase<Guid>
    {
        public string Name { get; set; }
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; }
        public int Quantity { get; set; }
        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }
    }
}
