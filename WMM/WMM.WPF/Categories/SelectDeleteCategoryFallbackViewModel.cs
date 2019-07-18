using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WMM.WPF.MVVM;

namespace WMM.WPF.Categories
{
    public class SelectDeleteCategoryFallbackViewModel : ObservableObject
    {
        private CategoryViewModel _selectedFallbackCategory;

        public SelectDeleteCategoryFallbackViewModel(IEnumerable<CategoryViewModel> categories, CategoryViewModel deletedCategory)
        {
            DeletedCategory = deletedCategory;
            Categories = categories.ToList();
            Categories.Remove(deletedCategory);
        }

        public List<CategoryViewModel> Categories { get; }

        public CategoryViewModel DeletedCategory { get; }

        public CategoryViewModel SelectedFallbackCategory
        {
            get => _selectedFallbackCategory;
            set => SetValue(ref _selectedFallbackCategory, value);
        }
    }
}
