using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;

namespace WMM.WPF.Goals
{
    public static class GoalCalculator
    {
        public static GoalMonthInfo CalculateGoalMonthInfo(Goal goal, DateTime month, List<Transaction> transactions)
        {
            var info = new GoalMonthInfo();

            CalculateIdeal(transactions, goal, month, info);
            CalculateActual(transactions, month, info);
            CalculateStatus(goal, month, info);

            return info;
        }
        
        private static void CalculateIdeal(List<Transaction> transactions, Goal goal, DateTime month, GoalMonthInfo info)
        {
            var initialAmount = transactions.Where(x => x.Recurring).Select(x => x.Amount).Sum();
            var endAmount = goal.Limit;

            var startDate = month.FirstDayOfMonth();
            var endDate = month.LastDayOfMonth();
            var slope = (endAmount - initialAmount) / (endDate.Subtract(startDate).Days + 1); // +1: start amount applies at start of day 1 and not at end so day 1 counts

            var points = new List<DateAmountPoint>
            {
                new DateAmountPoint(startDate, initialAmount + slope),
                new DateAmountPoint(endDate, endAmount)
            };

            var currentDate = DateTime.Now.Date;
            if (currentDate < endDate && currentDate > startDate)
            {
                
                var currentAmount = (currentDate.Subtract(startDate).Days + 1) * slope;
                points.Add(new DateAmountPoint(currentDate, currentAmount));
                info.CurrentIdealAmount = currentAmount;
            }
            else
            {
                info.CurrentIdealAmount = endAmount;
            }

            info.IdealPoints = points;
        }

        private static void CalculateActual(List<Transaction> transactions, DateTime month, GoalMonthInfo info)
        {
            var points = new List<DateAmountPoint>();
            var date = month.FirstDayOfMonth();
            var lastDate = (DateTime.Now.Date < month.LastDayOfMonth()) ? DateTime.Now.Date : month.LastDayOfMonth();
            var cumulativeAmount = 0d;

            while (date <= lastDate)
            {
                var dayAmount = transactions.Where(x => x.Date.Date == date).Select(x => x.Amount).Sum();
                cumulativeAmount += dayAmount;
                points.Add(new DateAmountPoint(date, cumulativeAmount));
                date = date.AddDays(1);
            }

            info.CurrentAmount = cumulativeAmount;
            info.ActualPoints = points;
        }

        private static void CalculateStatus(Goal goal, DateTime month, GoalMonthInfo info)
        {
            if (DateTime.Now.Date > month.LastDayOfMonth()) // old month
            {
                info.Status = info.CurrentAmount < goal.Limit ? GoalStatus.Failed : GoalStatus.Success;
            }
            else // current month
            {
                info.Status = info.CurrentAmount < info.CurrentIdealAmount ? GoalStatus.OffTrack : GoalStatus.OnTrack;
            }
        }
    }
}
