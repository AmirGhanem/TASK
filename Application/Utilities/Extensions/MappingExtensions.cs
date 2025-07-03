using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Application.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Extensions
{
    public static class MappingExtensions
    {

        public static async Task<BasePaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize) where TDestination : class
        {
            return await BasePaginatedList<TDestination>.CreateAsync(queryable.AsNoTracking(), pageNumber, pageSize);
        }
        
        public static async Task<BasePaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(this List<TDestination> queryable, int pageNumber, int pageSize) where TDestination : class
        {
            return await BasePaginatedList<TDestination>.CreateAsync(queryable, pageNumber, pageSize);
        }
        public static async Task<BasePaginatedList<TDestination>> ToPaginatedListWithTrackingAsync<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize) where TDestination : class
        {
            return await BasePaginatedList<TDestination>.CreateAsync(queryable, pageNumber, pageSize);
        }
        public static void CreateMapFoPaginatedList<T>(this Profile profile)
        {
            profile.CreateMap<T, T>();
        }

        public static async Task<BasePaginatedList<TDestination>> ToPaginatedListAsync<TDestination>(
    this IQueryable<TDestination> queryable,
    IMapper mapper,
    int pageNumber,
    int pageSize)
    where TDestination : class
        {
            var paginatedList = await queryable.ProjectTo<TDestination>(mapper.ConfigurationProvider).ToListAsync();

            return new BasePaginatedList<TDestination>(
                paginatedList,
                paginatedList.Count,
                pageNumber,
                pageSize);
        }
    }
}
