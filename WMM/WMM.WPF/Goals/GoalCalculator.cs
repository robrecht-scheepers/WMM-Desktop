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
            info.InitialAmount = transactions.Where(x => x.Recurring || x.Category.CategoryType == CategoryType.Recurring).Select(x => x.Amount).Sum();
            var endAmount = goal.Limit;

            var startDate = month.FirstDayOfMonth();
            var endDate = month.LastDayOfMonth();
            var slope = (endAmount - info.InitialAmount) / (endDate.Subtract(startDate).Days + 1); 
            // +1: initialAmount applies at start of day 1 (so actually at day 0)

            var points = new List<DateAmountPoint>
            {
                new DateAmountPoint(startDate, info.InitialAmount + slope),
                new DateAmountPoint(endDate, endAmount)
            };

            var currentDate = DateTime.Now.Date;
            if (currentDate < endDate && currentDate > startDate)
            {
                var currentAmount = info.InitialAmount + (currentDate.Subtract(startDate).Days + 1) * slope;
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
                var marginLimit = goal.Limit - (info.InitialAmount - goal.Limit) * 0.1; // failed by <10% --> off track instead of failed
                
                info.Status = info.CurrentAmount >= goal.Limit ? GoalStatus.Success: (info.CurrentAmount < marginLimit ? GoalStatus.Failed : GoalStatus.OffTrack);
            }
            else // current month
            {
                info.Status = 
                    info.CurrentAmount < goal.Limit 
                        ? GoalStatus.Failed 
                        : info.CurrentAmount < info.CurrentIdealAmount 
                            ? GoalStatus.OffTrack 
                            : GoalStatus.OnTrack;
            }
        }
    }
}
