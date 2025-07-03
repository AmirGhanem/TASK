using AutoMapper;
using Domain.Identity;
using Domain.Models;
using Domain.Models.Contractors;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Models
{
    public class BaseMapperViewModel<T> : ModelBase<T>
    {
        public T Id { get; set; }
        [AdaptIgnore]
        public override ApplicationUser? CreatedBy { get; set; }
        public string? CreatedByFullName { get; set; }

        [AdaptIgnore]
        public override ApplicationUser? ModifiedBy { get; set; }
        public string? ModifiedByFullName { get; set; }
    }
}
