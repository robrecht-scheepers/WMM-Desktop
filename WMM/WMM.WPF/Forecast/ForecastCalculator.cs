using System;
using System.Collections.Generic;
using System.Linq;
using WMM.Data;
using WMM.WPF.Helpers;

namespace WMM.WPF.Forecast
{
    public class ForecastCalculator
    {
        public static double CalculateForecast(Category category, List<Transaction> history, DateTime date)
        {
            switch (category.ForecastType)
            {
                case ForecastType.Exception:
                    return CalculateActualTotal(category, history, date); // no forecast
                case ForecastType.Monthly:
                    var actual = CalculateActualTotal(category, history, date);
                    var mean = CalculateMonthlyMean(category, history, date);
                    return Math.Abs(actual) > Math.Abs(mean)
                           ? actual : mean;
                case ForecastType.Daily:
                    return CalculateDailyForecast(category, history, date);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static double CalculateActualTotal(Category category, List<Transaction> history, DateTime date)
        {
            return history.Where(t => 
                t.Date >= date.FirstDayOfMonth() &&
                t.Date <= date.LastDayOfMonth() &&
                t.Category == category.Name)
                .Select(x => x.Amount).Sum();
        }

        private static double CalculateMonthlyMean(Category category, List<Transaction> history, DateTime date)
        {
            var startDate = history.Select(x => x.Date).Min();
            var monthTotals = new List<double>();

            var month = date.PreviousMonth().FirstDayOfMonth();
            while (month >= startDate)
            {
                monthTotals.Add(history.Where(t => t.Date >= month && t.Date <= month.LastDayOfMonth() && t.Category == category.Name).Select(t => t.Amount).Sum());
                month = month.PreviousMonth().FirstDayOfMonth();
            }

            return monthTotals.Average();
        }

        private static double CalculateDailyForecast(Category category, List<Transaction> history, DateTime date)
        {
            // calculate daily mean from history
            var startDate = history.Select(x => x.Date).Min();
            var numberOfDays = Math.Ceiling((date - startDate).TotalDays);
            var total = history.Where(t => t.Date <= date && t.Category == category.Name).Select(t => t.Amount).Sum();
            var dailyMean = total / numberOfDays;

            // calculate current total
            var currentTotal = history.Where(t => t.Date >= date.FirstDayOfMonth() && t.Date <= date && t.Category == category.Name)
                .Select(x => x.Amount).Sum();
            
            // extrapolate for remainder of month
            var daysRemaining = Math.Ceiling((date.LastDayOfMonth() - date).TotalDays);

            return currentTotal + daysRemaining * dailyMean;
        }

    }
}
