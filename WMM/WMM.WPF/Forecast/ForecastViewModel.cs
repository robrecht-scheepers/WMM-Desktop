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
        public string Name { get; set; }
        public double CurrentAmount { get; set; }
        public double ForecastAmount { get; set; }
    }

    public class ForecastViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private ObservableCollection<ForecastLine> _forecastLines;
        private double _forecastTotal;
        private double _currentTotal;

        public ForecastViewModel(IRepository repository)
        {
            _repository = repository;
            ForecastLines = new ObservableCollection<ForecastLine>();
        }

        public ObservableCollection<ForecastLine> ForecastLines 
        {
            get => _forecastLines;
            set => SetValue(ref _forecastLines, value);
        }

        public double ForecastTotal
        {
            get => _forecastTotal;
            set => SetValue(ref _forecastTotal, value);
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
            double totalForecastAmount = 0.0;

            var areas = categories.Select(x => x.Area).Distinct().OrderBy(y => y);
            foreach (var area in areas)
            {
                var areaLines = new List<ForecastLine>();
                foreach (var category in categories.Where(x => x.Area == area).OrderBy(x => x.Name))
                {
                    var forecast = ForecastCalculator.CalculateForecast(category, history, DateTime.Today);
                    if (Math.Abs(forecast.Item1) > 0.0 || Math.Abs(forecast.Item2) > 0.0)
                    {
                        areaLines.Add(new ForecastLine{Name = category.Name, CurrentAmount = forecast.Item1, ForecastAmount = forecast.Item2});
                    }
                }

                if (areaLines.Any())
                {
                    var areaCurrentAmount = areaLines.Select(x => x.CurrentAmount).Sum();
                    var areaForecastAmount = areaLines.Select(x => x.ForecastAmount).Sum();

                    totalCurrentAmount += areaCurrentAmount;
                    totalForecastAmount += areaForecastAmount;
                    ForecastLines.Add(new ForecastLine{Name = area, CurrentAmount = areaCurrentAmount, ForecastAmount = areaForecastAmount});
                    foreach (var categoryLine in areaLines)
                    {
                        ForecastLines.Add(categoryLine);
                    }
                }
            }

            CurrentTotal = totalCurrentAmount;
            ForecastTotal = totalForecastAmount;
        }
    }
}
