using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;

namespace WMM.WPF.Goals
{
    public static class GoalCalculator
    {
        public static double CalculateCurrentGoalAmount(List<Transaction> transactions)
        {
            return transactions.Where(x => x.Date <= DateTime.Now).Select(x => x.Amount).Sum();
        }

        public static GetGoalStatus(Goal goal, List<Transaction> transactions)
        {

        }

        public static List<DateAmountPoint> GetIdealPoints(List<Transaction> transactions, Goal goal, DateTime month)
        {
            var startAmount = transactions.Where(x => x.Recurring).Select(x => x.Amount).Sum();
            var endAmount = goal.Limit;
            var startDate = month.FirstDayOfMonth();
            var endDate = month.LastDayOfMonth();

            var result = new List<DateAmountPoint>
            {
                new DateAmountPoint(startDate, startAmount),
                new DateAmountPoint(endDate, endAmount)
            };

            var currentDate = DateTime.Now.Date;
            if (currentDate < endDate)
            {
                var slope = (endAmount - startAmount) / endDate.Subtract(startDate).Days;
                var currentAmount = currentDate.Subtract(startDate).Days * slope;
                result.Add(new DateAmountPoint(currentDate, currentAmount));
            }

            return result;
        }

        public static List<DateAmountPoint> GetActualPoints(List<Transaction> transactions, DateTime month)
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

            return points;
        }
    }
}
