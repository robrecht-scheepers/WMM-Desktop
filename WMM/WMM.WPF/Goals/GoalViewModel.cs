using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Categories;
using WMM.WPF.Controls;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF.Goals
{
    public class GoalViewModel : ObservableObject
    {
        private Goal _goal;
        private readonly List<CategoryTypeSelectionItem> _categoryTypes;
        private readonly List<string> _areas;
        private readonly List<Category> _categories;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private List<ISelectableItem> _criteria;
        private string _name;
        private string _description;
        private double _limit;
        private AsyncRelayCommand _saveChangesCommand;
        private RelayCommand _resetCommand;

        public GoalViewModel(Goal goal, List<CategoryTypeSelectionItem> categoryTypes, List<string> areas, 
            List<Category> categories, List<ISelectableItem> criteria,
            IRepository repository, IWindowService windowService)
        {
            _goal = goal;
            _categoryTypes = categoryTypes;
            _areas = areas;
            _categories = categories;
            _repository = repository;
            _windowService = windowService;
            Criteria = criteria;

            Name = _goal.Name;
            Description = _goal.Description;
            Limit = _goal.Limit;
            SetSelectedCriteria();
        }

        public List<ISelectableItem> Criteria
        {
            get => _criteria;
            set => SetValue(ref _criteria, value);
        }

        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetValue(ref _description, value);
        }

        public double Limit
        {
            get => _limit;
            set => SetValue(ref _limit, value);
        }

        private void SetSelectedCriteria()
        {
            foreach (var criterion in Criteria)
            {
                criterion.IsSelected = false;
            }
            foreach (var categoryType in _goal.CategoryTypeCriteria)
            {
                Criteria.Cast<AreaCategoryMultiSelectionItem>().First(x =>
                    x.Item.SelectionType == AreaCategorySelectionItem.AreaCategorySelectionType.CategoryType &&
                    x.Item.Name == _categoryTypes.First(y => y.CategoryType == categoryType).Caption).IsSelected = true;
            }

            foreach (var areaCriterion in _goal.AreaCriteria)
            {
                Criteria.Cast<AreaCategoryMultiSelectionItem>().First(x =>
                    x.Item.SelectionType == AreaCategorySelectionItem.AreaCategorySelectionType.Area &&
                    x.Item.Name == areaCriterion).IsSelected = true;
            }

            foreach (var category in _goal.CategoryCriteria)
            {
                Criteria.Cast<AreaCategoryMultiSelectionItem>().First(x =>
                    x.Item.SelectionType == AreaCategorySelectionItem.AreaCategorySelectionType.Category &&
                    x.Item.Name == category.Name).IsSelected = true;
            }
        }

        public AsyncRelayCommand SaveChangesCommand => _saveChangesCommand ?? (_saveChangesCommand = new AsyncRelayCommand(SaveChanges));
        
        private async Task SaveChanges()
        {
            var typeCriteria = Criteria.Cast<AreaCategoryMultiSelectionItem>().Where(x =>
                    x.IsSelected && x.Item.SelectionType == AreaCategorySelectionItem.AreaCategorySelectionType.CategoryType)
                .Select(x => _categoryTypes.First(y => y.Caption == x.Item.Name).CategoryType).ToList();
            var areaCriteria = Criteria.Cast<AreaCategoryMultiSelectionItem>().Where(x =>
                    x.IsSelected && x.Item.SelectionType == AreaCategorySelectionItem.AreaCategorySelectionType.Area)
                .Select(x => x.Item.Name).ToList();
            var categoryCriteria = Criteria.Cast<AreaCategoryMultiSelectionItem>().Where(x =>
                    x.IsSelected && x.Item.SelectionType ==
                    AreaCategorySelectionItem.AreaCategorySelectionType.Category)
                .Select(x => _categories.First(y => y.Name == x.Item.Name)).ToList();

            _goal = await _repository.UpdateGoal(_goal, Name, Description, typeCriteria, areaCriteria, categoryCriteria, Limit);
        }

        public RelayCommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(Reset));

        private void Reset()
        {
            Name = _goal.Name;
            Description = _goal.Description;
            Limit = _goal.Limit;
            SetSelectedCriteria();
        }
    }
}
