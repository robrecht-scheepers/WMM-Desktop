using System;
using System.CodeDom;
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
    public class CategoryViewModel : ObservableObject
    {
        private string _area;
        private string _category;
        private string _newArea;
        private string _newCategory;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private AsyncRelayCommand _editCategoryCommand;
        private RelayCommand _resetCommand;

        public CategoryViewModel(IEnumerable<string> areas, IRepository repository, string area, string category, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;

            Areas = new ObservableCollection<string>(areas);
            Area = area;
            Category = category;
            NewArea = area;
            NewCategory = category;
        }

        public string Area
        {
            get => _area;
            set => SetValue(ref _area, value);
        }

        public string Category
        {
            get => _category;
            set => SetValue(ref _category, value);
        }

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

        public ObservableCollection<string> Areas { get; }

        public AsyncRelayCommand EditCategoryCommand => _editCategoryCommand ?? (_editCategoryCommand = new AsyncRelayCommand(EditCategory, CanExecuteEditCategory));

        private bool CanExecuteEditCategory()
        {
            return !string.IsNullOrEmpty(NewArea) 
                   && NewArea != Area
                   && !string.IsNullOrEmpty(NewCategory) 
                   && NewCategory != Category;
        }

        private async Task EditCategory()
        {
            try
            {
                await _repository.EditCategory(Category, NewArea, NewCategory);
            }
            catch (Exception e)
            {
                _windowService.ShowMessage($"Fehler aufgetreten: {e.Message}", "Fehler");
            }
            
            Category = NewCategory;
            Area = NewArea;
        }

        public RelayCommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(Reset));

        private void Reset()
        {
            NewCategory = Category;
            NewArea = Area;
        }
    }
}
