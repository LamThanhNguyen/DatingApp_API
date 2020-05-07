using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers {
    public class PagedList<T> : List<T> {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public PagedList (List<T> items, int count, int pageNumber, int pageSize) {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int) Math.Ceiling (count / (double) pageSize);
            this.AddRange (items);
            //this.AddRange(items) thêm vào cuối List trong List<T> các items
        }

        public static async Task<PagedList<T>> CreateAsync (IQueryable<T> source, int pageNumber, int pageSize) {
            var count = await source.CountAsync();
            var items = await source.Skip ((pageNumber - 1) * pageSize).Take (pageSize).ToListAsync();
            //Ta có Số trang = 3. pageSize = 5. 
            // ==> Skip(10). Ta lấy 5.
            //Ta có số trang = 7. pageSize = 8.
            // ==> Skip(48). Ta lấy 8.
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}


//Giả sử ta có source = 30 items.
//pageNumber = 6.
//pageSize = 5;
//==> CreateAsync( ==>5 items cuối,30, 6, 5);
// TotalCount = 30
//pageSize = 5
//CurrentPage = 6
//TotalPages = 6
