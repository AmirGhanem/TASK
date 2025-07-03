using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Extensions
{
    internal static class ExpressionExtensions
    {
        internal static Expression<Func<TIn, TOut>> CreatePropertyAccessor<TIn, TOut>(string propertyName)
        {
            var param = Expression.Parameter(typeof(TIn));
            var body = Expression.PropertyOrField(param, propertyName);
            return Expression.Lambda<Func<TIn, TOut>>(body, param);
        }

    }
}
