using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Resources;

namespace WMM.WPF.Forecast
{
    public class ForecastViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private ObservableCollection<ForecastLineGroup> _currentMonthForecastAreas;
        private double _currentMonthForecast;
        private double _currentMonthActual;
        private double _genericForecast;
        private ObservableCollection<ForecastLineGroup> _genericForecastAreas;
        private double _currentMonthDiff;
        private RelayCommand _excelExportCommand;

        public ForecastViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            CurrentMonthForecastAreas = new ObservableCollection<ForecastLineGroup>();
            GenericForecastAreas = new ObservableCollection<ForecastLineGroup>();
        }

        public ObservableCollection<ForecastLineGroup> CurrentMonthForecastAreas 
        {
            get => _currentMonthForecastAreas;
            set => SetValue(ref _currentMonthForecastAreas, value);
        }

        public ObservableCollection<ForecastLineGroup> GenericForecastAreas
        {
            get => _genericForecastAreas;
            set => SetValue(ref _genericForecastAreas, value);
        }

        public double CurrentMonthActual
        {
            get => _currentMonthActual;
            set => SetValue(ref _currentMonthActual, value);
        }

        public double CurrentMonthForecast
        {
            get => _currentMonthForecast;
            set => SetValue(ref _currentMonthForecast, value);
        }

        public double CurrentMonthDiff  
        {
            get => _currentMonthDiff;
            set => SetValue(ref _currentMonthDiff, value);
        }

        public double GenericForecast
        {
            get => _genericForecast;
            set => SetValue(ref _genericForecast,value);
        }

        public RelayCommand ExcelExportCommand => _excelExportCommand ?? (_excelExportCommand = new RelayCommand(ExcelExport));

        private void ExcelExport()
        {
            try
            {
                ExcelHelper.OpenInExcel(CurrentMonthForecastAreas, GenericForecastAreas);

            }
            catch (Exception e)
            {
                _windowService.ShowMessage(string.Format(Captions.ExcelError, e.Message), Captions.Error);
            }
        }

        public async Task Initialize()
        {
            var history = (await _repository.GetTransactions()).ToList();
            var categories = _repository.GetCategories();

            double currentMonthActual = 0.0;
            double currentMonthForecast = 0.0;
            
            double genericForecastTotal = 0.0;

            var areas = categories.Select(x => x.Area).Distinct().OrderBy(y => y);
            foreach (var area in areas)
            {
                var areaLinesCurrentMonth = new List<ForecastLine>();
                var areaLinesGeneric = new List<ForecastLine>();

                foreach (var category in categories.Where(x => x.Area == area).OrderBy(x => x.Name))
                {
                    var forecast = ForecastCalculator.CalculateCurrentMonthForecast(category, history, DateTime.Today);

                    currentMonthActual += forecast.Item1;
                    currentMonthForecast += forecast.Item2;
                    if (Math.Abs(forecast.Item1) > 0.0 || Math.Abs(forecast.Item2) > 0.0)
                    {
                        areaLinesCurrentMonth.Add(new ForecastLine{Name = category.Name, CurrentAmount = forecast.Item1, ForecastAmount = forecast.Item2});
                    }

                    var genericForecast = ForecastCalculator.CalculateGenericMonthForecast(category, history);
                    genericForecastTotal += genericForecast;
                    if (Math.Abs(genericForecast) > 0.0)
                    {
                        areaLinesGeneric.Add(new ForecastLine { Name = category.Name, CurrentAmount = 0.0, ForecastAmount = genericForecast});
                    }
                }

                if (areaLinesCurrentMonth.Any())
                {
                    var areaForecast = new ForecastLineGroup
                    {
                        Name = area,
                        CurrentAmount = areaLinesCurrentMonth.Select(x => x.CurrentAmount).Sum(),
                        ForecastAmount = areaLinesCurrentMonth.Select(x => x.ForecastAmount).Sum()
                    };
                    
                    foreach (var categoryLine in areaLinesCurrentMonth)
                    {
                        areaForecast.ForecastLines.Add(categoryLine);
                    }

                    CurrentMonthForecastAreas.Add(areaForecast);
                }

                if (areaLinesGeneric.Any())
                {
                    var areaForecast = new ForecastLineGroup
                    {
                        Name = area,
                        ForecastAmount = areaLinesGeneric.Select(x => x.ForecastAmount).Sum()
                    };
                    
                    foreach (var categoryLine in areaLinesGeneric)
                    {
                        areaForecast.ForecastLines.Add(categoryLine);
                    }

                    GenericForecastAreas.Add(areaForecast);
                }
            }

            CurrentMonthActual = currentMonthActual;
            CurrentMonthForecast = currentMonthForecast;
            CurrentMonthDiff = CurrentMonthForecast - CurrentMonthActual;

            GenericForecast = genericForecastTotal;
        }
    }
}
