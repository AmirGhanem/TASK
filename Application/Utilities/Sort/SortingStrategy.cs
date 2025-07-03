using Application.Utilities.Contractors;
using Application.Utilities.Extensions;
using Application.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Sort
{
    internal class SortingStrategy : ISortingStrategy
    {
        public IQueryable<TEntity> Sort<TEntity>(IQueryable<TEntity> items, List<SortedQuery> sortCriteria)
        {
            if (sortCriteria == null || sortCriteria.Count == 0)
                return items;

            var ordered = InnerSort(items, sortCriteria[0], true);
            for (var i = 1; i < sortCriteria.Count; ++i)
            {
                ordered = InnerSort(ordered, sortCriteria[i], false);
            }
            return ordered;
        }
        private static IOrderedQueryable<TEntity> InnerSort<TEntity>(IQueryable<TEntity> items, SortedQuery criteria, bool isMainSortProperty)
        {
            IOrderedQueryable<TEntity> ordered = null;

            var propertyInfo = typeof(TEntity).GetProperty(criteria.PropertyName, BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo == null)
            {
                return items.OrderBy(x => 0);
            }

            var propertyType = propertyInfo.PropertyType;


            if (propertyType.IsEnum || (Nullable.GetUnderlyingType(propertyType) != null && Nullable.GetUnderlyingType(propertyType).IsEnum))
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");

                var property = Expression.Property(parameter, propertyInfo);
                var propertyLambda = Expression.Lambda(property, parameter);

                // Create a dynamic sorting expression based on enum type
                var orderByMethod = isMainSortProperty ? "OrderBy" : "ThenBy";
                var orderByDirectionMethod = criteria.Direction == SortDirection.DESC ? $"{orderByMethod}Descending" : orderByMethod;
                var orderByExpression = Expression.Call(typeof(Queryable), orderByDirectionMethod, new[] { typeof(TEntity), propertyType }, items.Expression, propertyLambda);

                // Compile and apply the sorting expression
                var orderedQuery = items.Provider.CreateQuery<TEntity>(orderByExpression);
                ordered = (IOrderedQueryable<TEntity>)orderedQuery;
            }
            else if (propertyType.IsAssignableFrom(typeof(string)))
            {
                var propertyAccessor = ExpressionExtensions.CreatePropertyAccessor<TEntity, string>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy(items as IOrderedQueryable<TEntity>, propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(int?)))
            {
                var propertyAccessor = ExpressionExtensions.CreatePropertyAccessor<TEntity, int?>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy(items as IOrderedQueryable<TEntity>, propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(int)))
            {
                var propertyAccessor = ExpressionExtensions.CreatePropertyAccessor<TEntity, int>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy(items as IOrderedQueryable<TEntity>, propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(DateTime?)))
            {
                var propertyAccessor = ExpressionExtensions.CreatePropertyAccessor<TEntity, DateTime?>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy(items as IOrderedQueryable<TEntity>, propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(DateTime)))
            {
                var propertyAccessor = ExpressionExtensions.CreatePropertyAccessor<TEntity, DateTime>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy(items as IOrderedQueryable<TEntity>, propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(decimal?)))
            {
                var propertyAccessor = ExpressionExtensions.CreatePropertyAccessor<TEntity, decimal?>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy(items as IOrderedQueryable<TEntity>, propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(decimal)))
            {
                var propertyAccessor = ExpressionExtensions.CreatePropertyAccessor<TEntity, decimal>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy(items as IOrderedQueryable<TEntity>, propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(double?)))
            {
                var propertyAccessor = ExpressionExtensions.CreatePropertyAccessor<TEntity, double?>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy(items as IOrderedQueryable<TEntity>, propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(double)))
            {
                var propertyAccessor = ExpressionExtensions.CreatePropertyAccessor<TEntity, double>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy(items as IOrderedQueryable<TEntity>, propertyAccessor, criteria.Direction);
            }

            if (ordered == null)
                throw new ArgumentException(string.Format("Unable to sort by the requested criteria: {0} - {1}", criteria.PropertyName, criteria.Direction), "criteria");

            return ordered;
        }

        private static IOrderedQueryable<T> OrderBy<T, TProperty>(IQueryable<T> items, Expression<Func<T, TProperty>> expression, SortDirection direction)
        {
            switch (direction)
            {
                case SortDirection.ASC:
                default:
                    return items.OrderBy(expression);
                case SortDirection.DESC:
                    return items.OrderByDescending(expression);
            }
        }

        private static IOrderedQueryable<T> ThenBy<T, TProperty>(IOrderedQueryable<T> items, Expression<Func<T, TProperty>> expression, SortDirection direction)
        {
            switch (direction)
            {
                case SortDirection.ASC:
                default:
                    return items.ThenBy(expression);
                case SortDirection.DESC:
                    return items.ThenByDescending(expression);
            }
        }
    }
}
