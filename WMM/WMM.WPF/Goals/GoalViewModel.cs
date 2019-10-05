using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Categories;
using WMM.WPF.Controls;
using WMM.WPF.Forecast;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF.Goals
{
    public class GoalViewModel : ObservableObject
    {
        
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
        private string _criteriaString;

        public GoalViewModel(Goal goal, List<CategoryTypeSelectionItem> categoryTypes, List<string> areas, 
            List<Category> categories, List<ISelectableItem> criteria,
            IRepository repository, IWindowService windowService)
        {
            Goal = goal;
            _categoryTypes = categoryTypes;
            _areas = areas;
            _categories = categories;
            _repository = repository;
            _windowService = windowService;
            Criteria = criteria;

            Name = Goal.Name;
            Description = Goal.Description;
            Limit = Goal.Limit;
            CriteriaString = CreateCriteriaString(Goal);
            SetSelectedCriteria();
        }

        public Goal Goal { get; private set; }

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

        public string CriteriaString
        {
            get => _criteriaString;
            set => SetValue(ref _criteriaString, value);
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
            foreach (var categoryType in Goal.CategoryTypeCriteria)
            {
                Criteria.Cast<AreaCategoryMultiSelectionItem>().First(x =>
                    x.Item.SelectionType == AreaCategorySelectionItem.AreaCategorySelectionType.CategoryType &&
                    x.Item.Name == _categoryTypes.First(y => y.CategoryType == categoryType).Caption).IsSelected = true;
            }

            foreach (var areaCriterion in Goal.AreaCriteria)
            {
                Criteria.Cast<AreaCategoryMultiSelectionItem>().First(x =>
                    x.Item.SelectionType == AreaCategorySelectionItem.AreaCategorySelectionType.Area &&
                    x.Item.Name == areaCriterion).IsSelected = true;
            }

            foreach (var category in Goal.CategoryCriteria)
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

            Goal = await _repository.UpdateGoal(Goal, Name, Description, typeCriteria, areaCriteria, categoryCriteria, Limit);
        }

        public RelayCommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(Reset));

        private void Reset()
        {
            Name = Goal.Name;
            Description = Goal.Description;
            Limit = Goal.Limit;
            SetSelectedCriteria();
        }

        private string CreateCriteriaString(Goal goal)
        {
            var stringBuilder = new StringBuilder();
            var first = true;
            foreach (var categoryType in goal.CategoryTypeCriteria)
            {
                if (!first)
                    stringBuilder.Append(", ");
                stringBuilder.Append(categoryType.ToCaption());
                first = false;
            }

            foreach (var area in goal.AreaCriteria)
            {
                if (!first)
                    stringBuilder.Append(", ");
                stringBuilder.Append(area);
                first = false;
            }

            foreach (var category in goal.CategoryCriteria)
            {
                if (!first)
                    stringBuilder.Append(", ");
                stringBuilder.Append(category.Name);
                first = false;
            }

            return stringBuilder.ToString();
        }
    }
}
