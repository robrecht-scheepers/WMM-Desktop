﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private ForecastType _newForecastType;
        private AsyncRelayCommand<CategoryViewModel> _deleteCategiryCommand;

        public ManageCategoriesViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            Areas = new ObservableCollection<string>();
            Categories = new ObservableCollection<CategoryViewModel>();
            ForecastTypes = new ObservableCollection<ForecastType> { ForecastType.Exception, ForecastType.Monthly, ForecastType.Daily };
        }

        public void Initialize()
        {
            var categories = _repository.GetCategories();
            Areas = new ObservableCollection<string>(_repository.GetAreas().OrderBy(x => x));
            
            foreach (var area in Areas)
            {
                foreach (var category in categories.Where(x => x.Area == area).OrderBy(x => x.Name))
                {
                    Categories.Add(new CategoryViewModel(category, Areas, _repository, _windowService));
                }
            }
        }

        public ObservableCollection<string> Areas
        {
            get => _areas;
            set => SetValue(ref _areas, value);
        }

        public ObservableCollection<CategoryViewModel> Categories { get; }

        public ObservableCollection<ForecastType> ForecastTypes { get; }

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

        public ForecastType NewForecastType
        {
            get => _newForecastType;
            set => SetValue(ref _newForecastType, value);
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
                await _repository.AddCategory(AreaForNewCategory, NewCategory, NewForecastType);
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
            AreaForNewCategory = NewArea;
            NewArea = "";
        }

        public AsyncRelayCommand<CategoryViewModel> DeleteCategiryCommand => _deleteCategiryCommand ?? (_deleteCategiryCommand = new AsyncRelayCommand<CategoryViewModel>(DeleteCategory));

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
                await _repository.DeleteCategory(category.Name);
            }
        }
    }
}
