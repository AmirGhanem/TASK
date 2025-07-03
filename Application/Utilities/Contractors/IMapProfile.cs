using AutoMapper;
using Application.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Contractors
{
    internal interface IMapProfile
    {
        IMappingExpression<BasePaginatedList<T>, BasePaginatedList<T>> GetBasePaginatedListConfiguration<T>();
    }
}
