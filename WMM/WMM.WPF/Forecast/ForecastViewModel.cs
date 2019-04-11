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
    public struct ForecastLine
    {
        public string Name;
        public double Amount;
    }

    public class ForecastViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private ObservableCollection<ForecastLine> _forecastLines;

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

        public async Task Initialize()
        {
            var history = (await _repository.GetTransactions()).ToList();
            var categories = _repository.GetCategories();

            var forecastLines = new ObservableCollection<ForecastLine>();
            double totalForecastAmount = 0.0;

            var areas = categories.Select(x => x.Area).Distinct().OrderBy(y => y);
            foreach (var area in areas)
            {
                var areaLines = new List<ForecastLine>();
                foreach (var category in categories.Where(x => x.Area == area).OrderBy(x => x))
                {
                    var forecastAmount = ForecastCalculator.CalculateForecast(category, history, DateTime.Today);
                    if (forecastAmount > 0.0)
                    {
                        areaLines.Add(new ForecastLine{Name = category.Name, Amount = forecastAmount});
                    }
                }

                if (forecastLines.Any())
                {
                    var areaAmount = areaLines.Select(x => x.Amount).Sum();
                    totalForecastAmount += areaAmount;
                    forecastLines.Add(new ForecastLine{Name = area, Amount = areaAmount});
                    foreach (var categoryLine in areaLines)
                    {
                        forecastLines.Add(categoryLine);
                    }
                }
            }
        }
    }
}
