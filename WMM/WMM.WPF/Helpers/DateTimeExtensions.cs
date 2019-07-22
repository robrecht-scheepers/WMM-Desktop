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

        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            // todo: make depending on first day of week system setting. Now we assume monday being first day of week.
            var dayOfWeek = ((int)dt.DayOfWeek - 1) % 7;    
            return dt.Date.AddDays(-1 * dayOfWeek);
        }

        public static DateTime LastDayOfWeek(this DateTime dt)
        {
            // todo: make depending on first day of week system setting. Now we assume monday being first day of week.
            var dayOfWeek = ((int)dt.DayOfWeek - 1) % 7;    
            return dt.Date.AddDays(6 - dayOfWeek);
        }

        public static DateTime FirstDayOfYear(this DateTime dt)
        {
            return new DateTime(dt.Year, 1, 1);
        }

        public static DateTime LastDayOfYear(this DateTime dt)
        {
            return new DateTime(dt.Year + 1, 1, 1).AddDays(-1);
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
