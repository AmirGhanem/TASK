using Microsoft.EntityFrameworkCore;
using Application.Utilities.Extensions;
using Application.Utilities.Models;
using Domain.Common;
using Domain.Contractors;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Domain.Models.Contractors;

namespace Application.Utilities.Extensions
{
    public static class ModelExtensions
    {

        public static IQueryable<T> IncludeAllLevels<T, TProperty>(
        this IQueryable<T> query,
        Expression<Func<T, TProperty>> navigationProperty
    ) where T : class
        {
            return IncludeAllLevelsRecursive(query, navigationProperty);
        }

        private static IQueryable<T> IncludeAllLevelsRecursive<T, TProperty>(
            IQueryable<T> query,
            Expression<Func<T, TProperty>> navigationProperty
        ) where T : class
        {
            var includeString = GetIncludeString(navigationProperty);
            var subQuery = query.Include(includeString);

            var subNavigationProperty = navigationProperty.Compile();
            var childQuery = subNavigationProperty(query.FirstOrDefault());

            if (childQuery != null /*&& childQuery.Any()*/)
            {
                return IncludeAllLevelsRecursive(subQuery, navigationProperty);
            }

            return subQuery;
        }

        private static string GetIncludeString<T, TProperty>(Expression<Func<T, TProperty>> navigationProperty)
        {
            var memberExpression = navigationProperty.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("Expression must be a MemberExpression");
            }

            return memberExpression.Member.Name;
        }

        private static List<TEntity> BuildTree<TEntity>(this List<TEntity> nodes)
            where TEntity : class
        {
            var nodeLookup = nodes.ToDictionary(GetNodeId);
            var rootNodes = new List<TEntity>();

            foreach (var node in nodes)
            {
                var parentId = GetParentId(node);
                if (parentId.HasValue && nodeLookup.TryGetValue(parentId.Value, out var parent))
                {
                    SetChildrenList(parent, new List<TEntity> { node });
                }
                else
                {
                    rootNodes.Add(node);
                }
            }

            return rootNodes;
        }

        private static List<TProjection> SelectNodes<TEntity, TProjection>(
            List<TEntity> nodes,
            Expression<Func<TEntity, TProjection>> selector)
            where TEntity : class
            where TProjection : class
        {
            var selectedNodes = new List<TProjection>();

            foreach (var node in nodes)
            {
                var selectedNode = selector.Compile()(node);
                selectedNodes.Add(selectedNode);

                var childrenList = GetChildrenList(node);
                if (childrenList != null && childrenList.Any())
                {
                    var selectedChildren = SelectNodes(childrenList, selector);
                    SetChildrenList(selectedNode, selectedChildren);
                }
            }

            return selectedNodes;
        }

        // Helper methods to get/set properties based on expressions
        private static int? GetNodeId<TEntity>(TEntity entity) where TEntity : class => entity.GetType().GetProperty("Id")?.GetValue(entity) as int?;
        private static int? GetParentId<TEntity>(TEntity entity) where TEntity : class => entity.GetType().GetProperty("ParentId")?.GetValue(entity) as int?;
        private static List<TEntity> GetChildrenList<TEntity>(TEntity entity) where TEntity : class => entity.GetType().GetProperty("Children")?.GetValue(entity) as List<TEntity>;
        private static void SetChildrenList<TEntity>(TEntity entity, List<TEntity> childrenList) where TEntity : class => entity.GetType().GetProperty("Children")?.SetValue(entity, childrenList);





        public static IQueryable<TEntity> Include<TEntity>(
         this IQueryable<TEntity> source,
         int level,
         Expression<Func<TEntity, object>> expression)
         where TEntity : class
        {
            if (level < 0) level = source.Count();

            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("Expression must be a MemberExpression");
            }

            var propertyName = memberExpression.Member.Name;

            // Use reflection to get the property info
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("Expression must be a property expression");
            }

            // Build the include path
            var includePath = new StringBuilder(propertyName);
            for (int i = 1; i < level; i++)
            {
                includePath.Append($".{propertyName}");
            }

            // Use the include path with ThenInclude
            return source.Include(includePath.ToString());
        }

        public static void RemoveRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                source.Remove(item);
            }
        }
        public static async Task<List<TProjection>> LoadAllItemsWithSubitemsAsync<TEntity, TCollection, TProjection>(
    this DbContext dbContext,
    TEntity entity,
    Expression<Func<TEntity, TCollection>> navigationProperty,
    Expression<Func<TEntity, TProjection>> selector)
    where TEntity : class
    where TCollection : ICollection<TEntity>
        {
            var navigationPropertyName = GetPropertyName(navigationProperty);

            var collectionProperty = dbContext.Entry(entity).Collection(navigationPropertyName);

            if (collectionProperty != null)
            {
                await collectionProperty.LoadAsync();

                // Assuming your navigation property is a collection, you can access it like this:
                var subitems = navigationProperty.Compile()(entity);

                // Convert subitems to a flat list
                var flatSubitems = subitems.AsQueryable().Select(selector).ToList();

                // Recursively load subitems for each subitem
                foreach (var subitem in subitems.ToList())
                {
                    var subitemSubitems = await LoadAllItemsWithSubitemsAsync(dbContext, subitem, navigationProperty, selector);
                    flatSubitems.AddRange(subitemSubitems);
                }

                return flatSubitems;
            }

            return new List<TProjection> { selector.Compile()(entity) };
        }

        public static async Task<List<TEntity>> LoadAllItemsWithSubitemsAsync<TEntity, TCollection>(
        this DbContext dbContext, TEntity entity, Expression<Func<TEntity, TCollection>> navigationProperty)
        where TEntity : class
        where TCollection : ICollection<TEntity>
        {
            var navigationPropertyName = GetPropertyName(navigationProperty);

            var collectionProperty = dbContext.Entry(entity).Collection(navigationPropertyName);

            if (collectionProperty != null)
            {
                await collectionProperty.LoadAsync();

                // Assuming your navigation property is a collection, you can access it like this:
                var subitems = navigationProperty.Compile()(entity);

                // Convert subitems to a flat list
                var flatSubitems = subitems.ToList();

                // Recursively load subitems for each subitem
                foreach (var subitem in flatSubitems.ToList())
                {
                    var subitemSubitems = await LoadAllItemsWithSubitemsAsync(dbContext, subitem, navigationProperty);
                    flatSubitems.AddRange(subitemSubitems);
                }

                return flatSubitems;
            }

            return new List<TEntity> { entity };
        }

        public static async Task LoadChildEntitiesAsync<TEntity, TCollection>(
    this DbContext dbContext, TEntity entity, Expression<Func<TEntity, TCollection>> navigationProperty)
    where TEntity : class
    where TCollection : ICollection<TEntity>
        {
            var navigationPropertyName = GetPropertyName(navigationProperty);

            var collectionProperty = dbContext.Entry(entity).Collection(navigationPropertyName);

            if (collectionProperty != null)
            {
                await collectionProperty.LoadAsync();

                // Assuming your navigation property is a collection, you can access it like this:
                var childEntities = navigationProperty.Compile()(entity);

                // Recursively load child entities
                foreach (var childEntity in childEntities)
                {
                    await dbContext.LoadChildEntitiesAsync(childEntity, navigationProperty);
                }
            }
        }

        public static string GetPropertyName<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Expression is not a member access expression.");
            return memberExpression.Member.Name;
        }

        public static async Task LoadChildEntitiesAsync<T>(this T entity, DbContext dbContext) where T : class
        {
            // Find the navigation property for the child entities
            var navigationProperty = dbContext.Entry(entity)
                .Collections
                .FirstOrDefault(x => x.Metadata.ClrType == typeof(T));

            if (navigationProperty != null)
            {
                await navigationProperty.LoadAsync();

                // Assuming your navigation property is a collection, you can access it like this:
                var childEntities = (ICollection<T>)navigationProperty.CurrentValue;

                foreach (var childEntity in childEntities)
                {
                    await childEntity.LoadChildEntitiesAsync(dbContext);
                }
            }
        }

        public static TResult EntityExists<TEntity, TKey, TResult>(this DbContext dbContext, TKey key) where TEntity : ModelBase<TKey> where TResult : BaseCommandResult, new()
        {
            var entity = dbContext.Set<TEntity>().Find(key);
            if (entity == null)
            {
                return new TResult { ErrorCode = ErrorCode.NotFound, IsSuccess = false };
            }
            else
            {
                return null;
            }
        }


        public static IQueryable<T> Search<T>(this IQueryable<T> records, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return records;

            var propertyTypesToSearch = new Type[] { typeof(string), typeof(int?), typeof(int), typeof(Enum) };

            var properties = typeof(T)
                .GetProperties()
                .Where(prop =>
                    !Attribute.IsDefined(prop, typeof(NotMappedAttribute)) && // Check for [NotMapped] attribute
                    propertyTypesToSearch.Contains(prop.PropertyType) &&
                    !prop.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase));

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression body = Expression.Constant(false); // Initialize with false

            foreach (var property in properties)
            {
                Expression propertyExpression = Expression.Property(parameter, property);

                if (property.PropertyType == typeof(int) || property.PropertyType == typeof(Enum))
                {
                    propertyExpression = Expression.Call(propertyExpression, typeof(object).GetMethod("ToString"));
                }
                else if (property.PropertyType == typeof(int?))
                {
                    // Add a condition to check if the int? has a value before searching
                    var hasValueExpression = Expression.Property(propertyExpression, "HasValue");
                    var valueExpression = Expression.Property(propertyExpression, "Value");
                    propertyExpression = Expression.Condition(hasValueExpression, Expression.Call(valueExpression, typeof(object).GetMethod("ToString")), Expression.Constant(string.Empty));
                }
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var searchExpression = Expression.Constant(searchText.Trim(), typeof(string));
                var condition = Expression.Call(propertyExpression, containsMethod, searchExpression);

                body = Expression.OrElse(body, condition);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            return records.Where(lambda).Distinct();
        }


        public static bool AllHaveNotValue(params object[] values)
        {
            return values.All(value => value == null);
        }
        public static bool AllHaveValue(params object[] values)
        {
            return values.All(value => value != null);
        }
    }
}
