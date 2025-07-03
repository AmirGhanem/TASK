using Application.Utilities.Sort;
using Application.Utilities.Contractors;
using Application.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Sort
{
    public static class Sorting
    {
        private static ISortingStrategy Helper;

        static Sorting()
        {
            Helper = new SortingStrategy();
        }

        public static IQueryable<TEntity> Sort<TEntity>(this IQueryable<TEntity> items, List<SortedQuery> sortCriteria)
        {
            return Helper.Sort(items, sortCriteria);
        }
    }
}
