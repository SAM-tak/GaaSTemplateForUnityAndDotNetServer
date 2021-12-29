using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YourGameServer
{
    public interface IPaginatedList
    {
        int PageIndex { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }

    public class PagingNavigationModel
    {
        public PagingNavigationModel(IPaginatedList list)
        {
            this.list = list;
        }
        public PagingNavigationModel(string aspPage, IPaginatedList list)
        {
            this.aspPage = aspPage;
            this.list = list;
        }
        public string aspPage = "./Index";
        public IPaginatedList list;
    }

    public class PaginatedList<T> : List<T>, IPaginatedList
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage { get => PageIndex > 1; }

        public bool HasNextPage { get => PageIndex < TotalPages; }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
