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
    public class ManageGoalsViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private ObservableCollection<CategoryTypeSelectionItem> _categoryTypes;
        private ObservableCollection<string> _areas;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<GoalViewModel> _goalViewModels;
        

        public ManageGoalsViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            Areas = new ObservableCollection<string>();
            Categories = new ObservableCollection<Category>();
            CategoryTypes = new ObservableCollection<CategoryTypeSelectionItem>(CategoryTypeSelectionItem.GetList());
        }

        public ObservableCollection<CategoryTypeSelectionItem> CategoryTypes
        {
            get => _categoryTypes;
            set => SetValue(ref _categoryTypes, value);
        }

        public ObservableCollection<string> Areas
        {
            get => _areas;
            set => SetValue(ref _areas, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetValue(ref _categories, value);
        }

        public ObservableCollection<GoalViewModel> GoalViewModels
        {
            get => _goalViewModels;
            set => SetValue(ref _goalViewModels, value);
        }

        public async Task Initialize()
        {

            foreach (var goal in (await _repository.GetGoals()).OrderBy(x => x.Name))
            {
                GoalViewModels.Add(new GoalViewModel(goal, CategoryTypes, Areas, Categories, _repository, _windowService));
            }
        }
    }
}
