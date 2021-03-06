﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Forecast;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Resources;

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
        private CategoryType _newCategoryType;
        private AsyncRelayCommand<CategoryViewModel> _deleteCategoryCommand;

        public ManageCategoriesViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            Areas = new ObservableCollection<string>();
            Categories = new ObservableCollection<CategoryViewModel>();
            CategoryTypes = new ObservableCollection<CategoryTypeSelectionItem>(CategoryTypeSelectionItem.GetList());
        }

        public void Initialize()
        {
            var categories = _repository.GetCategories();
            Areas = new ObservableCollection<string>(_repository.GetAreas().OrderBy(x => x));
            
            Categories.Clear();
            foreach (var area in Areas)
            {
                foreach (var category in categories.Where(x => x.Area == area).OrderBy(x => x.Name))
                {
                    Categories.Add(new CategoryViewModel(category, Areas, CategoryTypes, _repository, _windowService));
                }
            }
        }

        public ObservableCollection<string> Areas
        {
            get => _areas;
            set => SetValue(ref _areas, value);
        }

        public ObservableCollection<CategoryViewModel> Categories { get; }

        public ObservableCollection<CategoryTypeSelectionItem> CategoryTypes { get; }

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

        public CategoryType NewCategoryType
        {
            get => _newCategoryType;
            set => SetValue(ref _newCategoryType, value);
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
                await _repository.AddCategory(AreaForNewCategory, NewCategory, NewCategoryType);
            }
            catch (Exception e)
            {
                _windowService.ShowMessage(string.Format(Captions.ErrorMessage, e.Message), Captions.Error);
                return;
            }
            
            Initialize();
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
                _windowService.ShowMessage(string.Format(Captions.ErrorMessage, e.Message), Captions.Error);
                return;
            }

            Areas = new ObservableCollection<string>(_repository.GetAreas().OrderBy(x => x));
            AreaForNewCategory = NewArea;
            NewArea = "";
        }

        public AsyncRelayCommand<CategoryViewModel> DeleteCategoryCommand => _deleteCategoryCommand ?? (_deleteCategoryCommand = new AsyncRelayCommand<CategoryViewModel>(DeleteCategory));

        private async Task DeleteCategory(CategoryViewModel category)
        {
            var transactions = await _repository.GetTransactions(new SearchConfiguration { CategoryName = category.Name});

            if (transactions.Any())
            {
                var selectFallbackViewModel = new SelectDeleteCategoryFallbackViewModel(Categories, category);
                _windowService.OpenDialogWindow(selectFallbackViewModel);

                if(!selectFallbackViewModel.Confirmed || selectFallbackViewModel.SelectedFallbackCategory == null)
                    return;

                var fallback = selectFallbackViewModel.SelectedFallbackCategory;
                await _repository.DeleteCategory(category.Name, fallback.Name);
            }
            else
            {
                if (!_windowService.AskConfirmation(Captions.ConfirmDeleteCategory))
                    return;
                await _repository.DeleteCategory(category.Name);
            }

            Initialize();
        }
    }
}
