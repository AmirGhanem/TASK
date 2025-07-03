using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities.Models
{
    public class BasePaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageNumber { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }


        public BasePaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = Math.Max((int)Math.Ceiling(count / (double)pageSize), 0);
            TotalCount = count;
            Items = items;
        }

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;

        public static async Task<BasePaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Max(pageSize, 1);
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new BasePaginatedList<T>(items, count, pageNumber, pageSize);
        }
        
        public static async Task<BasePaginatedList<T>> CreateAsync(List<T> source, int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Max(pageSize, 1);
            var count =  source.Count;
            var items =  source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new BasePaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
