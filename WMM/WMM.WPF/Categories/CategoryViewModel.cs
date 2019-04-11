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
        private string _name;
        private ForecastType _forecastType;
        private string _editedArea;
        private string _editedName;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private AsyncRelayCommand _editCategoryCommand;
        private RelayCommand _resetCommand;
        private ForecastType _editedForecastType;

        public CategoryViewModel(ObservableCollection<string> areas, IRepository repository, string area, string name, IWindowService windowService)
        {
            Areas = areas;
            Area = area;
            Name = name;
            EditedArea = area;
            EditedName = name;
        }

        public CategoryViewModel(Category category, ObservableCollection<string> areas, IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;

            Areas = areas;
            ForecastTypes = new ObservableCollection<ForecastType>{ForecastType.Exception, ForecastType.Monthly, ForecastType.Daily};

            Area = category.Area;
            Name = category.Name;
            ForecastType = category.ForecastType;

            EditedArea = Area;
            EditedName = Name;
            EditedForecastType = ForecastType;
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

        public ForecastType ForecastType
        {
            get => _forecastType;
            private set => SetValue(ref _forecastType, value);
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

        public ForecastType EditedForecastType
        {
            get => _editedForecastType;
            set => SetValue(ref _editedForecastType, value);
        }

        public ObservableCollection<string> Areas { get; }

        public ObservableCollection<ForecastType> ForecastTypes { get; }

        public AsyncRelayCommand EditCategoryCommand => _editCategoryCommand ?? (_editCategoryCommand = new AsyncRelayCommand(EditCategory, CanExecuteEditCategory));

        private bool CanExecuteEditCategory()
        {
            return !string.IsNullOrEmpty(EditedArea)
                    && !string.IsNullOrEmpty(EditedName)  
                    && (EditedArea != Area || EditedName != Name || EditedForecastType != ForecastType);
        }

        private async Task EditCategory()
        {
            try
            {
                await _repository.EditCategory(Name, EditedArea, EditedName, EditedForecastType);
            }
            catch (Exception e)
            {
                _windowService.ShowMessage($"Fehler aufgetreten: {e.Message}", "Fehler");
            }
            
            Name = EditedName;
            Area = EditedArea;
            ForecastType = EditedForecastType;
        }

        public RelayCommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(Reset));

        private void Reset()
        {
            EditedName = Name;
            EditedArea = Area;
            EditedForecastType = ForecastType;
        }
    }
}
