using Application.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Contractors
{
    internal interface IFilteringStrategy
    {
        IQueryable<TEntity> Filter<TEntity>(IQueryable<TEntity> query, List<FilteredQuery> filters);
    }
}
