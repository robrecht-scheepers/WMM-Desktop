using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Categories;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF.Goals
{
    public class GoalViewModel : ObservableObject
    {
        public ObservableCollection<AreaCategoryMultiSelectionItem> AreaCategorySelectionItems { get; }
        private readonly Goal _goal;
        private readonly List<CategoryTypeSelectionItem> _categoryTypes;
        private readonly List<string> _areas;
        private readonly List<Category> _categories;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;

        public GoalViewModel(Goal goal, List<CategoryTypeSelectionItem> categoryTypes, List<string> areas, 
            List<Category> categories, ObservableCollection<AreaCategoryMultiSelectionItem> areaCategorySelectionItems,
            IRepository repository, IWindowService windowService)
        {
            _goal = goal;
            _categoryTypes = categoryTypes;
            _areas = areas;
            _categories = categories;
            _repository = repository;
            _windowService = windowService;
            AreaCategorySelectionItems = areaCategorySelectionItems;
        }
    }
}
