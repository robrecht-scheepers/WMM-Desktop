using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Controls;
using WMM.WPF.Resources;

namespace WMM.WPF.Categories
{
    public class AreaCategoryMultiSelectionItem : ISelectableItem
    {
        private bool _isSelected;
        public AreaCategorySelectionItem Item { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsSelectable => Item.IsSelectable;
        public string Caption => Item.Name;

        public AreaCategoryMultiSelectionItem(AreaCategorySelectionItem item)
        {
            Item = item;
        }

        public event EventHandler SelectionChanged;
    }

    public class AreaCategorySelectionItem
    {
        public enum AreaCategorySelectionType { Area, Category, CategoryType }

        public AreaCategorySelectionType SelectionType { get; set; }
        public string Name { get; set; }
        public bool IsSelectable { get; set; }

        public AreaCategorySelectionItem(string name, AreaCategorySelectionType selectionType, bool isSelectable = true)
        {
            SelectionType = selectionType;
            Name = name;
            IsSelectable = isSelectable;
        }

        public static AreaCategorySelectionItem Empty => new AreaCategorySelectionItem("", AreaCategorySelectionType.Area);

        public static ObservableCollection<AreaCategorySelectionItem> GetList(IRepository repository, bool includeEmpty)
        {
            var areaCategoryList = new ObservableCollection<AreaCategorySelectionItem>();

            if (includeEmpty)
            {
                areaCategoryList.Add(Empty);
            }
            areaCategoryList.Add(new AreaCategorySelectionItem($"--- {Captions.CategoryType} ---", AreaCategorySelectionItem.AreaCategorySelectionType.CategoryType, false));
            foreach (var categoryTypeSelectionItem in CategoryTypeSelectionItem.GetList())
            {
                areaCategoryList.Add(new AreaCategorySelectionItem(categoryTypeSelectionItem.Caption, AreaCategorySelectionItem.AreaCategorySelectionType.CategoryType));
            }
            areaCategoryList.Add(new AreaCategorySelectionItem($"--- {Captions.Area} ---", AreaCategorySelectionItem.AreaCategorySelectionType.Area, false));
            foreach (var area in repository.GetAreas().OrderBy(x => x))
            {
                areaCategoryList.Add(new AreaCategorySelectionItem(area, AreaCategorySelectionItem.AreaCategorySelectionType.Area));
            }
            areaCategoryList.Add(new AreaCategorySelectionItem($"--- {Captions.Category} ---", AreaCategorySelectionItem.AreaCategorySelectionType.Category, false));
            foreach (var category in repository.GetCategoryNames().OrderBy(x => x))
            {
                areaCategoryList.Add(new AreaCategorySelectionItem(category, AreaCategorySelectionItem.AreaCategorySelectionType.Category));
            }

            return areaCategoryList;
        }
    }
}
