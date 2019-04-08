using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF.Categories
{
    public class ManageCategoriesViewModel : ObservableObject
    {
        private string _areaForNewCategory;
        private string _newCategory;
        private AsyncRelayCommand _addNewCategoryCommand;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private AsyncRelayCommand _addNewAreaCommand;
        private string _newArea;
        private ObservableCollection<string> _areas;

        public ManageCategoriesViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            Areas = new ObservableCollection<string>();
            Categories = new ObservableCollection<CategoryViewModel>();
        }

        public void Initialize()
        {
            Areas = new ObservableCollection<string>(_repository.GetAreas().OrderBy(x => x));
            var categories = _repository.GetAreasAndCategories();
            
            foreach (var area in Areas)
            {
                foreach (var category in categories[area].OrderBy(x => x))
                {
                    Categories.Add(new CategoryViewModel(Areas, _repository, area, category, _windowService));
                }
            }
        }

        public ObservableCollection<string> Areas
        {
            get => _areas;
            set => SetValue(ref _areas, value);
        }

        public ObservableCollection<CategoryViewModel> Categories { get; }

        public string AreaForNewCategory
        {
            get => _areaForNewCategory;
            set => SetValue(ref _areaForNewCategory, value);
        }

        public string NewCategory
        {
            get => _newCategory;
            set => SetValue(ref _newCategory, value);
        }

        public string NewArea
        {
            get => _newArea;
            set => SetValue(ref _newArea, value);
        }

        public AsyncRelayCommand AddNewCategoryCommand => _addNewCategoryCommand ?? (_addNewCategoryCommand = new AsyncRelayCommand(AddNewCategory, CanExecuteAddNewCategory));

        private bool CanExecuteAddNewCategory()
        {
            return !string.IsNullOrEmpty(AreaForNewCategory) && !string.IsNullOrEmpty(NewCategory);
        }

        private async Task AddNewCategory()
        {
            try
            {
                await _repository.AddCategory(AreaForNewCategory, NewCategory);
            }
            catch (Exception e)
            {
                _windowService.ShowMessage($"Fehler aufgetreten: {e.Message}", "Fehler");
                return;
            }
            
            Categories.Add(new CategoryViewModel(Areas, _repository, AreaForNewCategory, NewCategory, _windowService));
            NewCategory = "";
        }

        public AsyncRelayCommand AddNewAreaCommand => _addNewAreaCommand ?? (_addNewAreaCommand = new AsyncRelayCommand(AddNewArea, CanExecuteAddNewArea));

        private bool CanExecuteAddNewArea()
        {
            return !string.IsNullOrEmpty(NewArea);
        }

        private async Task AddNewArea()
        {
            try
            {
                await _repository.AddArea(NewArea);
            }
            catch (Exception e)
            {
                _windowService.ShowMessage($"Fehler aufgetreten: {e.Message}", "Fehler");
                return;
            }

            Areas = new ObservableCollection<string>(_repository.GetAreas().OrderBy(x => x));
            NewArea= "";
        }
    }
}
