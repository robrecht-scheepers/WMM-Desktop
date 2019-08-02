using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Forecast;

namespace WMM.WPF.Categories
{
    public struct ForecastTypeSelectionItem
    {
        public static IEnumerable<ForecastTypeSelectionItem> GetList()
        {
            return new List<ForecastTypeSelectionItem>
            {
                new ForecastTypeSelectionItem(ForecastType.Exception, ForecastType.Exception.ToCaption()),
                new ForecastTypeSelectionItem(ForecastType.Monthly, ForecastType.Monthly.ToCaption()),
                new ForecastTypeSelectionItem(ForecastType.Daily, ForecastType.Daily.ToCaption()),
                new ForecastTypeSelectionItem(ForecastType.Recurring, ForecastType.Recurring.ToCaption()),
            };
        }

        public ForecastTypeSelectionItem(ForecastType forecastType, string caption)
        {
            ForecastType = forecastType;
            Caption = caption;
        }

        public ForecastType ForecastType { get; set; }
        public string Caption { get; set; }
    }
}
