using System.Collections.Generic;
using System.Web;
using OrphanedProperties.Models;

namespace OrphanedProperties.ViewModels
{
    /// <summary>
    /// View model class for Orphaned Properties admin plugin
    /// </summary>
    public class OrphanedPropertiesViewModels
    {
        public IEnumerable<OrphanedPropertyResult> OrphanedProperties { get; set; }

        public IEnumerable<int> Pages
        {
            get
            {
                var list2 = new List<int> { 1 };
                var list = list2;
                if (PageNumber - PageSize - 1 > 1)
                {
                    list.Add(0);
                }
                for (var i = PageNumber - PageSize; i <= PageNumber + PageSize; i++)
                {
                    if (i > 1 && i < TotalPagesCount)
                    {
                        list.Add(i);
                    }
                }
                if (PageNumber + PageSize + 1 < TotalPagesCount)
                {
                    list.Add(0);
                }
                if (TotalPagesCount > 1)
                {
                    list.Add(TotalPagesCount);
                }
                return list;
            }
        }

        public int TotalPagesCount => (TotalItemsCount - 1) / PageSize + 1;

        public int MaxIndexOfItem
        {
            get
            {
                if (PageNumber * PageSize <= TotalItemsCount)
                {
                    return PageNumber * PageSize;
                }

                return TotalItemsCount;
            }
        }

        public int MinIndexOfItem
        {
            get
            {
                if (TotalItemsCount <= 0)
                {
                    return 0;
                }

                return (PageNumber - 1) * PageSize + 1;
            }
        }

        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public int TotalItemsCount { get; set; }
        public string QueryString { get; set; }

        public string PageUrl(int page)
        {
            var qs = HttpUtility.ParseQueryString(QueryString);
            qs["page"] = page.ToString();
            return $"?{qs}";
        }
    }
}