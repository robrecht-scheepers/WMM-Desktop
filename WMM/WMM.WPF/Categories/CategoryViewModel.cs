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
        private string _editedArea;
        private string _editedCategory;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private AsyncRelayCommand _editCategoryCommand;
        private RelayCommand _resetCommand;

        public CategoryViewModel(ObservableCollection<string> areas, IRepository repository, string area, string category, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;

            Areas = areas;
            Area = area;
            Category = category;
            EditedArea = area;
            EditedCategory = category;
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

        public string EditedArea
        {
            get => _editedArea;
            set => SetValue(ref _editedArea, value);
        }

        public string EditedCategory
        {
            get => _editedCategory;
            set => SetValue(ref _editedCategory, value);
        }

        public ObservableCollection<string> Areas { get; }

        public AsyncRelayCommand EditCategoryCommand => _editCategoryCommand ?? (_editCategoryCommand = new AsyncRelayCommand(EditCategory, CanExecuteEditCategory));

        private bool CanExecuteEditCategory()
        {
            return !string.IsNullOrEmpty(EditedArea)
                    && !string.IsNullOrEmpty(EditedCategory)  
                    && (EditedArea != Area || EditedCategory != Category);
        }

        private async Task EditCategory()
        {
            try
            {
                await _repository.EditCategory(Category, EditedArea, EditedCategory);
            }
            catch (Exception e)
            {
                _windowService.ShowMessage($"Fehler aufgetreten: {e.Message}", "Fehler");
            }
            
            Category = EditedCategory;
            Area = EditedArea;
        }

        public RelayCommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(Reset));

        private void Reset()
        {
            EditedCategory = Category;
            EditedArea = Area;
        }
    }
}
