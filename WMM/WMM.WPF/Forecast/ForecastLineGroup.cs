using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.WPF.MVVM;

namespace WMM.WPF.Forecast
{
    public class ForecastLine : ObservableObject
    {
        public string Name { get; set; }
        public double CurrentAmount { get; set; }
        public double ForecastAmount { get; set; }
        public double Difference => ForecastAmount - CurrentAmount;
    }

    public class ForecastLineGroup: ForecastLine
    {
        private ObservableCollection<ForecastLine> _forecastLines;

        public ForecastLineGroup()
        {
            ForecastLines = new ObservableCollection<ForecastLine>();
        }

        public ObservableCollection<ForecastLine> ForecastLines 
        {
            get => _forecastLines;
            private set => SetValue(ref _forecastLines, value);
        }
    }
}
