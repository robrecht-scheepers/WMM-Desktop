using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Resources;

namespace WMM.WPF.Forecast
{
    public static class ForecastTypeExtensions
    {
        public static string ToCaption(this ForecastType forecastType)
        {
            switch (forecastType)
            {
                case ForecastType.Exception:
                    return Captions.ForecastTypeException;
                case ForecastType.Monthly:
                    return Captions.ForecastTypeMonthly;
                case ForecastType.Daily:
                    return Captions.ForecastTypeDaily;
                case ForecastType.Recurring:
                    return Captions.ForecastTypeRecurring;
                default:
                    throw new ArgumentOutOfRangeException(nameof(forecastType), forecastType, null);
            }
        }
    }
}
