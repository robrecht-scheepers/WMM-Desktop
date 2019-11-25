using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WMM.WPF.Goals
{
    public class ViewModeBoolConverter : IValueConverter
    {
        public GoalDetailViewMode TrueViewMode { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewMode = (GoalDetailViewMode) value;
            return viewMode == TrueViewMode;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isTrue = (bool) (value ?? false);
            switch (TrueViewMode)
            {
                case GoalDetailViewMode.Month:
                    return isTrue ? GoalDetailViewMode.Month : GoalDetailViewMode.Year;
                default:
                    return isTrue ? GoalDetailViewMode.Year : GoalDetailViewMode.Month;
            }
        }
    }
}
