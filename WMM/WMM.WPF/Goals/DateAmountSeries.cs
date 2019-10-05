using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WMM.WPF.Goals
{
    public class DateAmountSeries
    {
        public SolidColorBrush Brush { get; set; }

        public List<DateAmountPoint> Points { get; set; }
    }
}
