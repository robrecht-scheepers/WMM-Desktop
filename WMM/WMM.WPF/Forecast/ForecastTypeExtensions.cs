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
        public static string ToCaption(this CategoryType categoryType)
        {
            switch (categoryType)
            {
                case CategoryType.Exception:
                    return Captions.ForecastTypeException;
                case CategoryType.Monthly:
                    return Captions.ForecastTypeMonthly;
                case CategoryType.Daily:
                    return Captions.ForecastTypeDaily;
                case CategoryType.Recurring:
                    return Captions.ForecastTypeRecurring;
                default:
                    throw new ArgumentOutOfRangeException(nameof(categoryType), categoryType, null);
            }
        }
    }
}
