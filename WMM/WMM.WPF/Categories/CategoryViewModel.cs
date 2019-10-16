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
using WMM.WPF.Resources;

namespace WMM.WPF.Categories
{
    public class CategoryViewModel : ObservableObject
    {
        private string _area;
        private string _name;
        private CategoryType _categoryType;
        private string _editedArea;
        private string _editedName;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private AsyncRelayCommand _editCategoryCommand;
        private RelayCommand _resetCommand;
        private CategoryType _editedCategoryType;
        private string _categoryTypeCaption;
        
        public CategoryViewModel(Category category, ObservableCollection<string> areas,
            ObservableCollection<CategoryTypeSelectionItem> categoryTypes, IRepository repository,
            IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;

            Areas = areas;
            CategoryTypes = categoryTypes;

            Area = category.Area;
            Name = category.Name;
            CategoryType = category.CategoryType;
            UpdateCategoryTypeCaption();

            EditedArea = Area;
            EditedName = Name;
            EditedCategoryType = CategoryType;
        }

        public string Area
        {
            get => _area;
            private set => SetValue(ref _area, value);
        }

        public string Name
        {
            get => _name;
            private set => SetValue(ref _name, value);
        }

        public CategoryType CategoryType
        {
            get => _categoryType;
            private set => SetValue(ref _categoryType, value, UpdateCategoryTypeCaption);
        }

        private void UpdateCategoryTypeCaption()
        {
            CategoryTypeCaption = CategoryTypes.First(x => x.CategoryType == CategoryType).Caption;
        }

        public string CategoryTypeCaption
        {
            get => _categoryTypeCaption;
            set => SetValue(ref _categoryTypeCaption, value);
        }

        public string EditedArea
        {
            get => _editedArea;
            set => SetValue(ref _editedArea, value);
        }

        public string EditedName
        {
            get => _editedName;
            set => SetValue(ref _editedName, value);
        }

        public CategoryType EditedCategoryType
        {
            get => _editedCategoryType;
            set => SetValue(ref _editedCategoryType, value);
        }

        public ObservableCollection<string> Areas { get; }

        public ObservableCollection<CategoryTypeSelectionItem> CategoryTypes { get; }

        public AsyncRelayCommand EditCategoryCommand => _editCategoryCommand ?? (_editCategoryCommand = new AsyncRelayCommand(EditCategory, CanExecuteEditCategory));

        private bool CanExecuteEditCategory()
        {
            return !string.IsNullOrEmpty(EditedArea)
                    && !string.IsNullOrEmpty(EditedName)  
                    && (EditedArea != Area || EditedName != Name || EditedCategoryType != CategoryType);
        }

        private async Task EditCategory()
        {
            try
            {
                await _repository.EditCategory(Name, EditedArea, EditedName, EditedCategoryType);
            }
            catch (Exception e)
            {
                _windowService.ShowMessage(string.Format(Captions.ErrorMessage, e.Message), Captions.Error);
            }
            
            Name = EditedName;
            Area = EditedArea;
            CategoryType = EditedCategoryType;
        }

        public RelayCommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(Reset));

        private void Reset()
        {
            EditedName = Name;
            EditedArea = Area;
            EditedCategoryType = CategoryType;
        }
    }
}
