using System;

namespace WMM.WPF.Helpers
{
    public static partial class DateTimeExtensions
    {
        public static DateTime FirstDayOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime dt)
        {
            return FirstDayOfMonth(dt).AddMonths(1).AddDays(-1);
        }

        public static DateTime PreviousMonth(this DateTime dt)
        {
            return dt.FirstDayOfMonth().AddDays(-1).FirstDayOfMonth();
        }

        public static DateTime PreviousMonth(this DateTime dt, int amount)
        {
            var tmpDate = dt;
            while (amount-- > 0)
            {
                tmpDate = tmpDate.PreviousMonth();
            }

            return tmpDate;
        }
    }
}
