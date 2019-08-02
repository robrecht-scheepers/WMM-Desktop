using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Forecast;

namespace WMM.WPF.Categories
{
    public struct CategoryTypeSelectionItem
    {
        public static IEnumerable<CategoryTypeSelectionItem> GetList()
        {
            return new List<CategoryTypeSelectionItem>
            {
                new CategoryTypeSelectionItem(CategoryType.Exception, CategoryType.Exception.ToCaption()),
                new CategoryTypeSelectionItem(CategoryType.Monthly, CategoryType.Monthly.ToCaption()),
                new CategoryTypeSelectionItem(CategoryType.Daily, CategoryType.Daily.ToCaption()),
                new CategoryTypeSelectionItem(CategoryType.Recurring, CategoryType.Recurring.ToCaption()),
            };
        }

        public CategoryTypeSelectionItem(CategoryType categoryType, string caption)
        {
            CategoryType = categoryType;
            Caption = caption;
        }

        public CategoryType CategoryType { get; set; }
        public string Caption { get; set; }
    }
}
