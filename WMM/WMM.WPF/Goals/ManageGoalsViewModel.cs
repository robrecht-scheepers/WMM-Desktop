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
        private readonly List<CategoryTypeSelectionItem> _categoryTypes;
        private List<string> _areas;
        private List<Category> _categories;
        private ObservableCollection<GoalViewModel> _goalViewModels;
        private ObservableCollection<AreaCategoryMultiSelectionItem> _areaCategorySelectionItems;
        private string _newGoalName;
        private string _newGoalDescription;
        private double _newGoalLimit;
        private AsyncRelayCommand _addNewGoalCommand;

        

        public ManageGoalsViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            _areas = new List<string>();
            _categories = new List<Category>();
            _categoryTypes = new List<CategoryTypeSelectionItem>(CategoryTypeSelectionItem.GetList());
        }

        public ObservableCollection<GoalViewModel> GoalViewModels
        {
            get => _goalViewModels;
            set => SetValue(ref _goalViewModels, value);
        }

        public ObservableCollection<AreaCategoryMultiSelectionItem> AreaCategorySelectionItems
        {
            get => _areaCategorySelectionItems;
            set => SetValue(ref _areaCategorySelectionItems, value);
        }

        public string NewGoalName
        {
            get => _newGoalName;
            set => SetValue(ref _newGoalName, value);
        }

        public string NewGoalDescription
        {
            get => _newGoalDescription;
            set => SetValue(ref _newGoalDescription, value);
        }

        public double NewGoalLimit
        {
            get => _newGoalLimit;
            set => SetValue(ref _newGoalLimit, value);
        }

        public async Task Initialize()
        {
            _areas = _repository.GetAreas().ToList();
            _categories = _repository.GetCategories();
            AreaCategorySelectionItems = new ObservableCollection<AreaCategoryMultiSelectionItem>(
                AreaCategorySelectionItem.GetList(_repository, false).Select(x => new AreaCategoryMultiSelectionItem(x)));

            foreach (var goal in (await _repository.GetGoals()).OrderBy(x => x.Name))
            {
                GoalViewModels.Add(new GoalViewModel(goal, _categoryTypes, _areas, _categories, AreaCategorySelectionItems, _repository, _windowService));
            }
        }

        public AsyncRelayCommand AddNewGoalCommand => _addNewGoalCommand ?? (_addNewGoalCommand = new AsyncRelayCommand(AddNewGoal));

        private async Task AddNewGoal()
        {
            try
            {
                var selectedCategoryTypes = AreaCategorySelectionItems.Where(x =>
                    x.IsSelected && x.Item.SelectionType == AreaCategorySelectionItem.AreaCategorySelectionType.CategoryType)
                    .Select(x => _categoryTypes.First(y => y.Caption == x.Item.Name).CategoryType).ToList();
                var selectedAreas = AreaCategorySelectionItems.Where(x =>
                        x.IsSelected && x.Item.SelectionType == AreaCategorySelectionItem.AreaCategorySelectionType.Area)
                    .Select(x => x.Item.Name).ToList();
                var selectedCategories = AreaCategorySelectionItems.Where(x =>
                        x.IsSelected && x.Item.SelectionType == AreaCategorySelectionItem.AreaCategorySelectionType.Category)
                    .Select(x => _categories.First(y => y.Name == x.Item.Name)).ToList();

                await _repository.AddGoal(NewGoalName, NewGoalDescription, selectedCategoryTypes, selectedAreas,
                    selectedCategories, NewGoalLimit);

                await Initialize();
            }
            catch (Exception e)
            {
                _windowService.ShowMessage($"Fehler aufgetreten: {e.Message}", "Fehler");
                return;
            }

            NewGoalName = "";
            NewGoalDescription = "";
            NewGoalLimit = 0d;
        }
    }
}
