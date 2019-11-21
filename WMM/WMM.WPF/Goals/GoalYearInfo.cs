using System.Collections.Generic;

namespace WMM.WPF.Goals
{
    public class GoalYearInfo
    {
        public List<MonthAmountPoint> MonthAmountPoints { get; set; }
        public double Average { get; set; }
        public double Limit { get; set; }

        public GoalYearInfo()
        {
            MonthAmountPoints = new List<MonthAmountPoint>();
        }
    }
}
