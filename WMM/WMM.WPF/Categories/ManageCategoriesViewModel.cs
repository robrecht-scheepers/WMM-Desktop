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
        private string _newArea;
        private string _newCategory;
        private AsyncRelayCommand _addNewCategoryCommand;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private List<string> _areas;

        public ManageCategoriesViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            Areas = new ObservableCollection<string>();
            Categories = new ObservableCollection<CategoryViewModel>();
        }

        public void Initialize()
        {
            var categories = _repository.GetAreasAndCategories();
            _areas = categories.Keys.OrderBy(x => x).ToList();

            foreach (var area in _areas)
            {
                Areas.Add(area);
                foreach (var category in categories[area].OrderBy(x => x))
                {
                    Categories.Add(new CategoryViewModel(_areas, _repository, area, category, _windowService));
                }
            }
        }

        public ObservableCollection<string> Areas { get; }

        public ObservableCollection<CategoryViewModel> Categories { get; }

        public string NewArea
        {
            get => _newArea;
            set => SetValue(ref _newArea, value);
        }

        public string NewCategory
        {
            get => _newCategory;
            set => SetValue(ref _newCategory, value);
        }

        public AsyncRelayCommand AddNewCategoryCommand => _addNewCategoryCommand ?? (_addNewCategoryCommand = new AsyncRelayCommand(AddNewCategory, CanExecuteAddNewCategory));

        private bool CanExecuteAddNewCategory()
        {
            return !string.IsNullOrEmpty(NewArea) && !string.IsNullOrEmpty(NewCategory);
        }

        private async Task AddNewCategory()
        {
            try
            {
                await _repository.AddCategory(NewArea, NewCategory);
            }
            catch (Exception e)
            {
                _windowService.ShowMessage($"Fehler aufgetreten: {e.Message}", "Fehler");
            }
            
            Categories.Add(new CategoryViewModel(_areas, _repository, NewArea, NewCategory, _windowService));
            NewCategory = "";
        }
    }
}
