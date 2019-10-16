using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;

namespace WMM.WPF.Goals
{
    public class GoalMonthInfo
    {
        public double InitialAmount { get; set; }
        public double CurrentAmount { get; set; }
        public double CurrentIdealAmount { get; set; }
        public GoalStatus Status { get; set; }
        public List<DateAmountPoint> IdealPoints { get; set; }
        public List<DateAmountPoint> ActualPoints { get; set; }
    }
}
