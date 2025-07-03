using Application.Utilities.Filter;
using Application.Utilities.Contractors;
using Application.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Filter
{
    public static class Filtering
    {
        private static readonly Dictionary<Type, Func<string, object>> DefaultValueParserMap = new Dictionary<Type, Func<string, object>>
        {
            [typeof(string)] = v => v,
            [typeof(int)] = v => int.Parse(v),
            [typeof(int?)] = v => !string.IsNullOrEmpty(v) ? int.Parse(v) : (int?)null,
            [typeof(DateTime)] = v => DateTime.Parse(v, CultureInfo.InvariantCulture),
            [typeof(DateTime?)] = v => !string.IsNullOrEmpty(v) ? DateTime.Parse(v, CultureInfo.InvariantCulture) : (DateTime?)null,
            [typeof(Guid)] = v => Guid.Parse(v),
            [typeof(Guid?)] = v => !string.IsNullOrEmpty(v) ? Guid.Parse(v) : (Guid?)null,
        };



        public static IQueryable<TEntity> Filter<TEntity>(this IQueryable<TEntity> query, List<FilteredQuery> filters)
        {
            return new FilteringStrategy().Filter(query, filters);
        }


    }
}
