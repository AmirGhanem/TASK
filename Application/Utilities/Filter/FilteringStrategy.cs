using LinqKit;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using Application.Utilities.Contractors;
using Application.Utilities.Extensions;
using Application.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Filter
{
    internal class FilteringStrategy : IFilteringStrategy
    {
        public IQueryable<TEntity> Filter<TEntity>(IQueryable<TEntity> query, List<FilteredQuery> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return query;
            }

            var predicate = PredicateBuilder.New<TEntity>(true);

            foreach (var filter in filters)
            {
                filter.Values = filter.Values.Select(x => x.Trim());
                var filterExpression = GetComparer<TEntity>(filter);

                if (filter.Operator == FilterOperator.And)
                {
                    predicate = predicate.And(filterExpression);
                }
                else if (filter.Operator == FilterOperator.Or)
                {
                    predicate = predicate.Or(filterExpression);
                }
            }

            return query.Where(predicate);
        }



        private Expression<Func<TEntity, bool>> GetComparer<TEntity>(FilteredQuery filter)
        {
            if (string.IsNullOrEmpty(filter.PropertyName) || !filter.Values.Any())
            {
                return entity => true;
            }

            var propertyType =
                typeof(TEntity).GetProperty(filter.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)?.PropertyType;
            if (propertyType == null)
            {
                return entity => true;
            }

            if (propertyType.IsAssignableFrom(typeof(string)))
            {
                return InnerGetComparer<TEntity, string, string>(filter.PropertyName, filter.Type, filter.Values);
            }
            if (propertyType.IsAssignableFrom(typeof(int)))
            {
                if (Nullable.GetUnderlyingType(propertyType).IsAssignableFrom(typeof(int)))
                {
                    return GetNullableIntFilter<TEntity>(filter.PropertyName, filter.Values);
                }
                return InnerGetComparer<TEntity, int?, int>(filter.PropertyName, filter.Type, filter.Values.Select(v => !string.IsNullOrWhiteSpace(v) ? new int?(int.Parse(v)) : null));
            }
            if (propertyType.IsAssignableFrom(typeof(DateTime?)))
            {
                return InnerGetComparer<TEntity, DateTime?, DateTime?>(filter.PropertyName, filter.Type,
                    filter.Values.Select(v => !string.IsNullOrWhiteSpace(v) ? new DateTime?(DateTime.Parse(v, CultureInfo.InvariantCulture)) : null));
            }
            if (propertyType.IsAssignableFrom(typeof(DateTime)))
            {
                return InnerGetComparer<TEntity, DateTime?, DateTime>(filter.PropertyName, filter.Type,
                    filter.Values.Select(v => !string.IsNullOrWhiteSpace(v) ? new DateTime?(DateTime.Parse(v, CultureInfo.InvariantCulture)) : null));
            }
            if (propertyType.IsAssignableFrom(typeof(bool)) || propertyType.IsAssignableFrom(typeof(bool?)))
            {
                return GetBoolFilter<TEntity>(filter.PropertyName, filter.Values);
            }
            if (propertyType.IsEnum)
            {
                return GetEnumFilter<TEntity>(filter.PropertyName, filter.Values, propertyType);
            }

            if (propertyType.IsValueType ||
    propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
     Nullable.GetUnderlyingType(propertyType)?.IsEnum == true)
            {
                if (propertyType == typeof(Guid))
                {
                    return GetGuidFilter<TEntity>(filter.PropertyName, filter.Values);
                }
                if (propertyType == typeof(Guid?))
                {
                    return GetNullableGuidFilter<TEntity>(filter.PropertyName, filter.Values);
                }
                return GetNullableEnumFilter<TEntity>(filter.PropertyName, filter.Values, propertyType);
            }

            throw new ArgumentException("Unexpected filter type " + filter.Type, "filter");
        }
        private Expression<Func<TEntity, bool>> GetGuidFilter<TEntity>(string propertyName, IEnumerable<string> values)
        {
            if (values == null || !values.Any())
            {
                return entity => true;
            }

            var parameterExp = Expression.Parameter(typeof(TEntity), "entity");
            var propertyExp = Expression.Property(parameterExp, propertyName);

            var guidValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v =>
                {
                    if (Guid.TryParse(v, out var guid))
                    {
                        return guid;
                    }
                    return Guid.Empty; // or any default value or handling for invalid Guids
                });

            var arrayExpression = Expression.NewArrayInit(typeof(Guid), guidValues.Select(value => Expression.Constant(value)));

            var containsMethod = typeof(Enumerable).GetMethods()
                .FirstOrDefault(method => method.Name == "Contains" && method.GetParameters().Length == 2)?
                .MakeGenericMethod(typeof(Guid));

            var containsCall = Expression.Call(null, containsMethod!, arrayExpression, propertyExp);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameterExp);

            return lambda;
        }

        private Expression<Func<TEntity, bool>> GetNullableGuidFilter<TEntity>(string propertyName, IEnumerable<string> values)
        {
            if (values == null || !values.Any())
            {
                return entity => true;
            }

            var parameterExp = Expression.Parameter(typeof(TEntity), "entity");
            var propertyExp = Expression.Property(parameterExp, propertyName);

            var guidValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v =>
                {
                    if (Guid.TryParse(v, out var guid))
                    {
                        return (Guid?)guid;
                    }
                    return null; // or any default value or handling for invalid Guids
                });

            var arrayExpression = Expression.NewArrayInit(typeof(Guid?), guidValues.Select(value => Expression.Constant(value, typeof(Guid?))));

            var containsMethod = typeof(Enumerable).GetMethods()
                .FirstOrDefault(method => method.Name == "Contains" && method.GetParameters().Length == 2)?
                .MakeGenericMethod(typeof(Guid?));

            var containsCall = Expression.Call(null, containsMethod!, arrayExpression, propertyExp);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameterExp);

            return lambda;
        }

        private static Expression<Func<TEntity, bool>> InnerGetComparer<TEntity, TPropertyPassed, TProperty>(string propertyName, FilterType filterType, IEnumerable<TPropertyPassed> values)
        {
            var valuesArray = values.ToArray();
            var selector = ExpressionExtensions.CreatePropertyAccessor<TEntity, TProperty>(propertyName);
            var propertyRef = selector.Body;
            var parameter = selector.Parameters[0];

            Expression<Func<TEntity, bool>> comparer = null;

            switch (filterType)
            {
                case FilterType.Equals:
                case FilterType.NotEquals:
                    comparer = BuildEqualityExpression<TEntity, TPropertyPassed>(valuesArray, propertyRef, parameter, filterType);
                    break;
                case FilterType.Range:
                    comparer = BuildRangeExpression<TEntity, TPropertyPassed>(valuesArray, propertyRef, parameter);
                    break;
                case FilterType.Contains:
                    comparer = BuildContainsExpression<TEntity, TPropertyPassed>(valuesArray, propertyRef, parameter);
                    break;
                case FilterType.LessThan:
                case FilterType.LessThanOrEqual:
                case FilterType.GreaterThan:
                case FilterType.GreaterThanOrEqual:
                    comparer = BuildSimpleComparisonExpression<TEntity, TPropertyPassed>(valuesArray, propertyRef, parameter, filterType);
                    break;
            }
            return comparer;
        }
        private Expression<Func<TEntity, bool>> GetNullableIntFilter<TEntity>(string propertyName, IEnumerable<string> values)
        {
            if (values == null || !values.Any())
            {
                return entity => true;
            }

            var parameterExp = Expression.Parameter(typeof(TEntity), "entity");
            var propertyExp = Expression.Property(parameterExp, propertyName);

            // Check if property is not null
            var nullCheck = Expression.NotEqual(propertyExp, Expression.Constant(null));

            Type underlyingType = Nullable.GetUnderlyingType(typeof(int?));

            var intValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => int.Parse(v));

            var arrayExpression = Expression.NewArrayInit(underlyingType, intValues.Select(value => Expression.Constant(value, underlyingType)));

            // Apply Contains only if the property is not null
            var containsMethod = typeof(Enumerable).GetMethods()?
                .FirstOrDefault(method => method.Name == "Contains" && method.GetParameters().Length == 2)?
                .MakeGenericMethod(underlyingType);

            // Create an expression for the nullable int's Value property
            var propertyValueExp = Expression.Property(propertyExp, "Value");

            // Apply Contains to the underlying type's Value property only if the property is not null
            var containsCall = Expression.Condition(
                nullCheck,
                Expression.Call(null, containsMethod!, arrayExpression, propertyValueExp),
                Expression.Constant(false)
            );

            var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameterExp);

            return lambda;
        }


        private Expression<Func<TEntity, bool>> GetEnumFilter<TEntity>(string propertyName, IEnumerable<string> values, Type enumType)
        {
            if (values == null || !values.Any())
            {
                return entity => true;
            }

            var parameterExp = Expression.Parameter(typeof(TEntity), "entity");
            var propertyExp = Expression.Property(parameterExp, propertyName);
            var propertyType = propertyExp.Type;

            var enumValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => Enum.Parse(enumType, v));

            var arrayExpression = Expression.NewArrayInit(enumType, enumValues.Select(value => Expression.Constant(value, enumType)));
            var containsMethod = typeof(Enumerable).GetMethods()?
                .FirstOrDefault(method => method.Name == "Contains" && method.GetParameters().Length == 2)?
                .MakeGenericMethod(enumType);

            var containsCall = Expression.Call(null, containsMethod!, arrayExpression, propertyExp);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameterExp);

            return lambda;
        }

        private Expression<Func<TEntity, bool>> GetNullableEnumFilter<TEntity>(string propertyName, IEnumerable<string> values, Type enumType)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (values == null || !values.Any())
            {
                return entity => true; // Return a predicate that always returns true if values are null or empty
            }

            var parameterExp = Expression.Parameter(typeof(TEntity), "entity");
            var propertyExp = Expression.Property(parameterExp, propertyName);

            // Check if property is not null
            var nullCheck = Expression.NotEqual(propertyExp, Expression.Constant(null));

            Type underlyingType = Nullable.GetUnderlyingType(enumType);
            if (underlyingType == null || !underlyingType.IsEnum)
            {
                throw new ArgumentException("Type provided must be a nullable Enum.", nameof(enumType));
            }

            // Parse enum values from string representation to Enum type
            var enumValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v =>
                {
                    if (v.Equals("null", StringComparison.OrdinalIgnoreCase))
                    {
                        return null; // Handle 'null' as a special case
                    }

                    if (Enum.TryParse(underlyingType, v, out var parsedValue))
                    {
                        return parsedValue;
                    }
                    else
                    {
                        throw new ArgumentException($"Value '{v}' is not a valid enum constant for type '{underlyingType}'.");
                    }
                })
                .ToArray(); // ToArray ensures evaluation outside the Expression tree

            // Create an array of Expression elements for the array initialization
            var arrayElements = enumValues.Select(value =>
            {
                if (value == null)
                {
                    // Handle null values by creating a ConstantExpression of null
                    return Expression.Constant(null, typeof(Nullable<>).MakeGenericType(underlyingType));
                }
                else
                {
                    // Create a ConstantExpression for non-null enum values
                    return Expression.Constant(value, typeof(Nullable<>).MakeGenericType(underlyingType));
                }
            });

            // Create the array initialization Expression
            var arrayExpression = Expression.NewArrayInit(typeof(Nullable<>).MakeGenericType(underlyingType), arrayElements);

            // Apply Contains only if the property is not null
            var containsMethod = typeof(Enumerable).GetMethods()
                .FirstOrDefault(method => method.Name == "Contains" && method.GetParameters().Length == 2)
                ?.MakeGenericMethod(typeof(Nullable<>).MakeGenericType(underlyingType));

            if (containsMethod == null)
            {
                throw new InvalidOperationException("Unable to find the Contains method.");
            }

            // Convert the property value expression to the correct nullable enum type
            var propertyValueExp = Expression.Convert(propertyExp, typeof(Nullable<>).MakeGenericType(underlyingType));

            // Apply Contains to the nullable enum type
            var containsCall = Expression.Call(null, containsMethod, arrayExpression, propertyValueExp);

            // Combine the null check and the contains check
            var combinedExpression = Expression.AndAlso(nullCheck, containsCall);

            // Create and return the lambda expression
            var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameterExp);
            return lambda;
        }






        //private Expression<Func<TEntity, bool>> GetNullableEnumFilter<TEntity>(string propertyName, IEnumerable<string> values, Type enumType)
        //{
        //    if (values == null || !values.Any())
        //    {
        //        return entity => true;
        //    }

        //    var parameterExp = Expression.Parameter(typeof(TEntity), "entity");
        //    var propertyExp = Expression.Property(parameterExp, propertyName);

        //    // Check if property is not null
        //    var nullCheck = Expression.NotEqual(propertyExp, Expression.Constant(null));

        //    Type underlyingType = Nullable.GetUnderlyingType(enumType);
        //    if (underlyingType == null || !underlyingType.IsEnum)
        //    {
        //        throw new ArgumentException("Type provided must be a nullable Enum.", nameof(enumType));
        //    }

        //    var enumValues = values
        //        .Where(v => !string.IsNullOrWhiteSpace(v))
        //        .Select(v => Enum.Parse(underlyingType, v));

        //    var arrayExpression = Expression.NewArrayInit(underlyingType, enumValues.Select(value => Expression.Constant(value, underlyingType)));

        //    // Apply Contains only if the property is not null
        //    var containsMethod = typeof(Enumerable).GetMethods()?
        //        .FirstOrDefault(method => method.Name == "Contains" && method.GetParameters().Length == 2)?
        //        .MakeGenericMethod(underlyingType);

        //    // Create an expression for the nullable enum's Value property
        //    var propertyValueExp = Expression.Property(propertyExp, "Value");

        //    // Apply Contains to the underlying type's Value property
        //    var containsCall = Expression.Call(null, containsMethod!, arrayExpression, propertyValueExp);

        //    // Combine the null check and the contains check
        //    var combinedExpression = Expression.AndAlso(nullCheck, containsCall);

        //    var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameterExp);

        //    return lambda;
        //}

        private static Expression<Func<TPoco, bool>> GetEqualsPredicate<TPoco>(string propertyName, object value, Type fieldType)
        {

            var parameterExp = Expression.Parameter(typeof(TPoco), @"t");   //(tpoco t)
            var propertyExp = Expression.Property(parameterExp, propertyName);// (tpoco t) => t.Propertyname

            var someValue = fieldType.IsEnum // get and eXpressionConstant.  Careful Enums must be reduced
                         ? Expression.Constant(Enum.ToObject(fieldType, value)) // Marc Gravell fix
                         : Expression.Constant(value, fieldType);

            var equalsExp = Expression.Equal(propertyExp, someValue); // yes this could 1 unreadble state if embedding someValue determination

            return Expression.Lambda<Func<TPoco, bool>>(equalsExp, parameterExp);
        }

        private static Expression<Func<TEntity, bool>> BuildEqualityExpression<TEntity, TPropertyPassed>(TPropertyPassed[] values,
                Expression propertyRef, ParameterExpression parameter, FilterType filterType)
        {
            BinaryExpression equalityAccumulator = null;
            foreach (var value in values)
            {
                var constantRef = Expression.Constant(value);
                BinaryExpression equalityExpression = null;
                switch (filterType)
                {
                    case FilterType.Equals:
                        equalityExpression = Expression.Equal(propertyRef, constantRef);
                        equalityAccumulator = equalityAccumulator != null ? Expression.Or(equalityAccumulator, equalityExpression) : equalityExpression;
                        break;
                    case FilterType.NotEquals:
                        equalityExpression = Expression.NotEqual(propertyRef, constantRef);
                        equalityAccumulator = equalityAccumulator != null ? Expression.And(equalityAccumulator, equalityExpression) : equalityExpression;
                        break;
                    default:
                        throw new ArgumentException("Unexpected FilterType " + filterType, "filterType");
                }

            }
            if (equalityAccumulator == null)
            {
                throw new ArgumentException("No values were provided", "values");
            }
            return Expression.Lambda<Func<TEntity, bool>>(equalityAccumulator, parameter);
        }


        private static Expression<Func<TEntity, bool>> BuildRangeExpression<TEntity, TPropertyPassed>(TPropertyPassed[] values,
          Expression propertyRef, ParameterExpression parameter)
        {
            BinaryExpression rangeAccumulator = null;
            if (values[0] != null)
            {
                var lowerBound = Expression.Constant(values[0]);
                rangeAccumulator = Expression.GreaterThanOrEqual(propertyRef, lowerBound);
            }
            if (values[1] != null)
            {
                var upperBound = Expression.Constant(values[1]);
                var upperExpression = Expression.LessThanOrEqual(propertyRef, upperBound);
                rangeAccumulator = rangeAccumulator != null
                    ? Expression.And(rangeAccumulator, upperExpression)
                    : upperExpression;
            }
            if (rangeAccumulator == null)
            {
                throw new ArgumentException("No values were provided", "values");
            }
            return Expression.Lambda<Func<TEntity, bool>>(rangeAccumulator, parameter);
        }

        private static Expression<Func<TEntity, bool>> BuildContainsExpression<TEntity, TPropertyPassed>(
            TPropertyPassed[] values, Expression propertyRef, ParameterExpression parameter)
        {

            var containsMethod = typeof(TPropertyPassed).GetMethod("Contains", new[] { typeof(string) });

            Expression containsAccumulator = null;
            foreach (var value in values)
            {
                var constantRef = Expression.Constant(value);
                var containsExpression = Expression.Call(propertyRef, containsMethod, constantRef);
                containsAccumulator = containsAccumulator != null ? Expression.Or(containsAccumulator, containsExpression) : containsExpression;
            }

            if (containsAccumulator == null)
            {
                throw new ArgumentException("No values were provided", "values");
            }

            return Expression.Lambda<Func<TEntity, bool>>(containsAccumulator, parameter);
        }

        private Expression<Func<TEntity, bool>> GetBoolFilter<TEntity>(string propertyName, IEnumerable<string> values)
        {
            if (values == null || !values.Any())
            {
                return entity => true;
            }

            var parameterExp = Expression.Parameter(typeof(TEntity), "entity");
            var propertyExp = Expression.Property(parameterExp, propertyName);

            var boolValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => bool.TryParse(v, out var result) ? (bool?)result : null);

            var arrayExpression = Expression.NewArrayInit(typeof(bool?), boolValues.Select(value => Expression.Constant(value, typeof(bool?))));
            var containsMethod = typeof(Enumerable).GetMethods()?
                .FirstOrDefault(method => method.Name == "Contains" && method.GetParameters().Length == 2)?
                .MakeGenericMethod(typeof(bool?));

            var containsCall = Expression.Call(null, containsMethod!, arrayExpression, propertyExp);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameterExp);

            return lambda;
        }


        private static Expression<Func<TEntity, bool>> BuildSimpleComparisonExpression<TEntity, TPropertyPassed>(
            TPropertyPassed[] values,
            Expression propertyRef, ParameterExpression parameter, FilterType filterType)
        {

            var constantRef = Expression.Constant(values.Single());
            BinaryExpression comparisonExpression = null;
            switch (filterType)
            {
                case FilterType.LessThan:
                    comparisonExpression = Expression.LessThan(propertyRef, constantRef);
                    break;
                case FilterType.LessThanOrEqual:
                    comparisonExpression = Expression.LessThanOrEqual(propertyRef, constantRef);
                    break;
                case FilterType.GreaterThan:
                    comparisonExpression = Expression.GreaterThan(propertyRef, constantRef);
                    break;
                case FilterType.GreaterThanOrEqual:
                    comparisonExpression = Expression.GreaterThanOrEqual(propertyRef, constantRef);
                    break;
                default:
                    throw new ArgumentException("Unexpected FilterType " + filterType, "filterType");
            }

            return Expression.Lambda<Func<TEntity, bool>>(comparisonExpression, parameter);
        }
    }
}
