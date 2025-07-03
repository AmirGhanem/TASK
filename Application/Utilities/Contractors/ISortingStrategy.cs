using Application.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Contractors
{
    internal interface ISortingStrategy
    {
        IQueryable<TEntity> Sort<TEntity>(IQueryable<TEntity> query, List<SortedQuery> sortCriteria);
    }
}
