using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WMM.WPF.Goals
{
    public class GoalDetailViewModeVisibilityConverter : IValueConverter
    {
        public GoalDetailViewMode VisibleMode { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is GoalDetailViewMode mode))
                return Visibility.Visible;

            return mode == VisibleMode ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
