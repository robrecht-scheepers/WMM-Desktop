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
        public ObservableCollection<string> Areas { get; }
        public ObservableCollection<CategoryTypeSelectionItem> CategoryTypes { get; }
        public ObservableCollection<Category> Categories { get; }
        private readonly Goal _goal;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;

        public GoalViewModel(Goal goal, ObservableCollection<CategoryTypeSelectionItem> categoryTypes, ObservableCollection<string> areas, 
            ObservableCollection<Category> categories, IRepository repository, IWindowService windowService)
        {
            _goal = goal;
            _repository = repository;
            _windowService = windowService;
            Areas = areas;
            CategoryTypes = categoryTypes;
            Categories = categories;
        }
    }
}
