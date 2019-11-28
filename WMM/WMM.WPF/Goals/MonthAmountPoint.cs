using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMM.WPF.Goals
{
    public class MonthAmountPoint
    {
        public DateTime Month { get; set; }
        public double Amount { get; set; }
        public GoalStatus Status { get; set; }
    }
}
