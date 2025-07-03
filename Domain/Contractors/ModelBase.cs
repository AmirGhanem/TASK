using Microsoft.VisualBasic;
using Domain.Common;
using Domain.Contractors;
using Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Domain.Models.Contractors
{
    public class ModelBase<T> : IModelBase<T>
    {
        [Key]
        public T Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser? CreatedBy { get; set; }
        [ForeignKey("CreatedBy")]
        public string? CreatedById { get; set; }

        public virtual ApplicationUser? ModifiedBy { get; set; }
        [ForeignKey("ModifiedBy")]
        public string? ModifiedById { get; set; }


    }
}
