using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return new DateTime(dt.Year, dt.Month + 1, 1).AddDays(-1);
        }

        public static DateTime PreviousMonth(this DateTime dt)
        {
            return dt.FirstDayOfMonth().AddDays(-1).FirstDayOfMonth();
        }
    }
}
