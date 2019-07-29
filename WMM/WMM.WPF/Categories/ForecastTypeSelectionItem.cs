using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;

namespace WMM.WPF.Categories
{
    public struct ForecastTypeSelectionItem
    {
        public ForecastTypeSelectionItem(ForecastType forecastType, string caption)
        {
            ForecastType = forecastType;
            Caption = caption;
        }

        public ForecastType ForecastType { get; set; }
        public string Caption { get; set; }
    }
}
