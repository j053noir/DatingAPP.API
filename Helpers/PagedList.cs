using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatinApp.API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        public PagedList(List<T> items, int totalRecords, int currentPage, int pageSize)
        {
            this.TotalRecords = totalRecords;
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
            this.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            this.AddRange(items);
        }

        public static async Task<PagedList<T>> CreateASync(IQueryable<T> source,
                                                           int currentPage,
                                                           int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((currentPage - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            return new PagedList<T>(items, count, currentPage, pageSize);
        }
    }
}
