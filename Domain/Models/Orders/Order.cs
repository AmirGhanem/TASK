using Domain.Common;
using Domain.Identity;
using Domain.Models.Contractors;
using Domain.Models.OrderItems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Orders
{
    public class Order : ModelBase<Guid>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int No { get; set; }
        public string CustomerName { get; set; }
        public OrderStatus Status { get; set; }
        public string? StatusChangedById { get; set; }
        public virtual ApplicationUser? StatusChangedBy { get; set; }
        public DateTime? StatusChangeDate { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
