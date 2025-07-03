using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using Domain.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Infrastructure.Common.Extensions
{
    public static class DatabaseContextExtensions
    {
        public static async Task AddOrUpdateAsync<TEntity>(this DbContext dbContext, TEntity entity)
    where TEntity : class
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            var entry = dbContext.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var primaryKey = dbContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties[0].Name;
                var keyValue = entity.GetType().GetProperty(primaryKey).GetValue(entity);

                var existingEntity = await dbContext.Set<TEntity>().FindAsync(keyValue);

                if (existingEntity != null)
                {
                    // Update the existing entity
                    dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
                }
                else
                {
                    // Add a new entity if it doesn't exist
                    dbContext.Set<TEntity>().Add(entity);
                }
            }
        }


        public static async Task AddIfNotExistsAsync<TEntity>(
    this DbContext dbContext, TEntity entity, string propertyName)
    where TEntity : class
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
            {
                throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found in the DbContext.");
            }

            var property = entityType.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException($"Property {propertyName} not found in entity type {typeof(TEntity).Name}.");
            }

            var propertyValue = property.PropertyInfo.GetValue(entity);
            var propertyType = property.ClrType;

            var dbSet = dbContext.Set<TEntity>();

            // Check if the entity is already being tracked
            var existingEntity = dbSet.Local.FirstOrDefault(e => property.PropertyInfo.GetValue(e).Equals(propertyValue));
            if (existingEntity != null)
            {
                // Entity with the specified property value already exists, update it if needed
                dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
            }
            else
            {
                // Entity with the specified property value doesn't exist, so add it
                await dbSet.AddAsync(entity);
            }
        }


        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (/*r.TargetEntry.State == EntityState.Added ||*/ r.TargetEntry.State == EntityState.Modified));
        public static string GetTableName<TEntity>(this ModelBuilder modelBuilder) where TEntity : class
        {
            var entityType = modelBuilder.Model.FindEntityType(typeof(TEntity));
            return entityType.GetTableName();
        }
        public static void CheckForTrim(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Iterate through all properties of the entity type
                foreach (var property in entityType.GetProperties())
                {
                    // Check if the property is a string
                    if (property.ClrType == typeof(string))
                    {
                        // Add a custom value converter to trim the string
                        var converter = new ValueConverter<string, string>(
                            v => (v == null) ? null : v.Trim(),
                            v => v
                        );

                        property.SetValueConverter(converter);
                    }
                }
            }
        }
        //public static void CheckForTrim(this ModelBuilder modelBuilder)
        //{
        //    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        //    {
        //        // Iterate through all properties of the entity type
        //        foreach (var property in entityType.GetProperties())
        //        {
        //            // Check if the property has the TrimAttribute applied
        //            if (property.PropertyInfo != null &&
        //                property.PropertyInfo.GetCustomAttribute<TrimAttribute>() != null)
        //            {
        //                // Add a custom value converter to trim the string
        //                var converter = new ValueConverter<string, string>(
        //                    v => v.Trim(),
        //                    v => v
        //                );

        //                property.SetValueConverter(converter);
        //            }
        //        }
        //    }
        //}
    }
}
