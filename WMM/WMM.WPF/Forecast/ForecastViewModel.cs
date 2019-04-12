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
        public double Amount { get; set; }
    }

    public class ForecastViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private ObservableCollection<ForecastLine> _forecastLines;
        private double _forecastTotal;

        public ForecastViewModel(IRepository _repository)
        {
            this._repository = _repository;
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

        public async Task Initialize()
        {
            var history = (await _repository.GetTransactions()).ToList();
            var categories = _repository.GetCategories();

            double totalForecastAmount = 0.0;

            var areas = categories.Select(x => x.Area).Distinct().OrderBy(y => y);
            foreach (var area in areas)
            {
                var areaLines = new List<ForecastLine>();
                foreach (var category in categories.Where(x => x.Area == area).OrderBy(x => x.Name))
                {
                    var forecastAmount = ForecastCalculator.CalculateForecast(category, history, DateTime.Today);
                    if (Math.Abs(forecastAmount) > 0.0)
                    {
                        areaLines.Add(new ForecastLine{Name = category.Name, Amount = forecastAmount});
                    }
                }

                if (areaLines.Any())
                {
                    var areaAmount = areaLines.Select(x => x.Amount).Sum();
                    totalForecastAmount += areaAmount;
                    ForecastLines.Add(new ForecastLine{Name = area, Amount = areaAmount});
                    foreach (var categoryLine in areaLines)
                    {
                        ForecastLines.Add(categoryLine);
                    }
                }
            }

            ForecastTotal = totalForecastAmount;
        }
    }
}
