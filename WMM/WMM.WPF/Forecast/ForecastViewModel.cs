using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.MVVM;

namespace WMM.WPF.Forecast
{
    public class ForecastLine
    {
        public string Area { get; set; }
        public string Category { get; set; }
        public double CurrentAmount { get; set; }
        public double ForecastAmount { get; set; }
        public double Difference => ForecastAmount - CurrentAmount;
    }

    public class ForecastViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private ObservableCollection<ForecastLine> _currentMonthForecastLines;
        private double _currentMonthForecastTotal;
        private double _currentTotal;
        private double _genericForecastTotal;
        private ObservableCollection<ForecastLine> _genericForecastLines;

        public ForecastViewModel(IRepository repository)
        {
            _repository = repository;
            CurrentMonthForecastLines = new ObservableCollection<ForecastLine>();
            GenericForecastLines = new ObservableCollection<ForecastLine>();
        }

        public ObservableCollection<ForecastLine> CurrentMonthForecastLines 
        {
            get => _currentMonthForecastLines;
            set => SetValue(ref _currentMonthForecastLines, value);
        }

        public ObservableCollection<ForecastLine> GenericForecastLines
        {
            get => _genericForecastLines;
            set => SetValue(ref _genericForecastLines, value);
        }

        public double CurrentMonthForecastTotal
        {
            get => _currentMonthForecastTotal;
            set => SetValue(ref _currentMonthForecastTotal, value);
        }

        public double GenericForecastTotal
        {
            get => _genericForecastTotal;
            set => SetValue(ref _genericForecastTotal,value);
        }

        public double CurrentTotal
        {
            get => _currentTotal;
            set => SetValue(ref _currentTotal, value);
        }

        public async Task Initialize()
        {
            var history = (await _repository.GetTransactions()).ToList();
            var categories = _repository.GetCategories();

            double totalCurrentAmount = 0.0;
            double totalCurrentForecastAmount = 0.0;

            double totalGenericForecastAmount = 0.0;

            var areas = categories.Select(x => x.Area).Distinct().OrderBy(y => y);
            foreach (var area in areas)
            {
                var areaLinesCurrentMonth = new List<ForecastLine>();
                var areaLinesGeneric = new List<ForecastLine>();

                foreach (var category in categories.Where(x => x.Area == area).OrderBy(x => x.Name))
                {
                    var forecast = ForecastCalculator.CalculateCurrentMonthForecast(category, history, DateTime.Today);
                    if (Math.Abs(forecast.Item1) > 0.0 || Math.Abs(forecast.Item2) > 0.0)
                    {
                        areaLinesCurrentMonth.Add(new ForecastLine{Area = area, Category = category.Name, CurrentAmount = forecast.Item1, ForecastAmount = forecast.Item2});
                    }

                    var genericForecast = ForecastCalculator.CalculateGenericMonthForecast(category, history);
                    if (Math.Abs(genericForecast) > 0.0)
                    {
                        areaLinesGeneric.Add(new ForecastLine { Area = area, Category = category.Name, CurrentAmount = 0.0, ForecastAmount = genericForecast});
                    }
                }

                if (areaLinesCurrentMonth.Any())
                {
                    var areaCurrentAmount = areaLinesCurrentMonth.Select(x => x.CurrentAmount).Sum();
                    var areaForecastAmount = areaLinesCurrentMonth.Select(x => x.ForecastAmount).Sum();

                    totalCurrentAmount += areaCurrentAmount;
                    totalCurrentForecastAmount += areaForecastAmount;
                    CurrentMonthForecastLines.Add(new ForecastLine{Area = area, Category = "", CurrentAmount = areaCurrentAmount, ForecastAmount = areaForecastAmount});
                    foreach (var categoryLine in areaLinesCurrentMonth)
                    {
                        CurrentMonthForecastLines.Add(categoryLine);
                    }
                }

                if (areaLinesGeneric.Any())
                {
                    var areaGenericAmount = areaLinesGeneric.Select(x => x.ForecastAmount).Sum();
                    totalGenericForecastAmount += areaGenericAmount;
                    GenericForecastLines.Add(new ForecastLine { Area = area, Category = "", CurrentAmount = 0.0, ForecastAmount = areaGenericAmount});
                    foreach (var categoryLine in areaLinesGeneric)
                    {
                        GenericForecastLines.Add(categoryLine);
                    }
                }
            }

            CurrentTotal = totalCurrentAmount;
            CurrentMonthForecastTotal = totalCurrentForecastAmount;
            GenericForecastTotal = totalGenericForecastAmount;
        }
    }
}
