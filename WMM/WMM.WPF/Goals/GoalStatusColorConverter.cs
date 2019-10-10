using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace WMM.WPF.Goals
{
    public class GoalStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is GoalStatus status))
                return Brushes.Black;

            switch (status)
            {
                case GoalStatus.OnTrack:
                    return Brushes.Green;
                case GoalStatus.OffTrack:
                    return Brushes.DarkOrange;
                case GoalStatus.Success:
                    return Brushes.DarkGreen;
                case GoalStatus.Failed:
                    return Brushes.Red;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
