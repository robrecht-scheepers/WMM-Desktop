using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.WPF.Properties;
using DateTime = System.DateTime;

namespace WMM.WPF.Helpers
{
    public static class SettingsHelper
    {
        private const string Separator = ";";

        public static string GetSelectedCurrency()
        {
            return Settings.Default.SelectedCurrency;
        }

        public static void SaveSelectedCurrency(string value)
        {
            Settings.Default.SelectedCurrency = value;
            Settings.Default.Save();
        }

        public static bool IsMonthExpanded(DateTime month)
        {
            return GetExpandedMonths().Contains(month.FirstDayOfMonth());
        }

        public static void SaveExpandedMonth(DateTime month)
        {
            var months = GetExpandedMonths();
            if (months.Contains(month))
                return;

            months.Add(month);
            Settings.Default.ExpandedMonthBalances = AggregateMonths(months);
            Settings.Default.Save();
        }

        public static void SaveCollapsedMonth(DateTime month)
        {
            var months = GetExpandedMonths();
            if (!months.Contains(month))
                return;

            months.Remove(month);
            Settings.Default.ExpandedMonthBalances = AggregateMonths(months);
            Settings.Default.Save();
        }

        private static string AggregateMonths(List<DateTime> months)
        {
            if (!months.Any())
                return "";

            return months.Select(x => x.ToString("yyyyMM")).Aggregate("", (a, v) => $"{a}{v}{Separator}");
        }

        private static List<DateTime> GetExpandedMonths()
        {
            var expandedMonthsSetting = Settings.Default.ExpandedMonthBalances;
            var months = new List<DateTime>();

            foreach (var monthEntry in expandedMonthsSetting.Split(';').Where(x => !string.IsNullOrEmpty(x)))
            {
                months.Add(new DateTime(int.Parse(monthEntry.Substring(0, 4)), int.Parse(monthEntry.Substring(4, 2)), 1));
            }

            return months;
        }


    }
}
