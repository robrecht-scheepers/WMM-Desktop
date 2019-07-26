using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using WMM.Data;
using WMM.WPF.Helpers;

namespace WMM.WPF.Forecast
{
    public class ForecastCalculator
    {
        public static (double,double) CalculateCurrentMonthForecast(Category category, List<Transaction> history, DateTime date)
        {
            if (!history.Any(x => x.Category == category))
                return (0.0, 0.0);

            var actual = CalculateActualTotal(category, history, date);
            double forecast;
            switch (category.ForecastType)
            {
                case ForecastType.Exception:
                    forecast = actual; // no forecast
                    break;
                case ForecastType.Monthly:
                    var mean = CalculateMonthlyMean(category, history, date);
                    forecast =  Math.Abs(actual) > Math.Abs(mean)
                           ? actual : mean;
                    break;
                case ForecastType.Daily:
                    forecast = CalculateDailyForecast(category, history, date);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (actual, forecast);
        }

        public static double CalculateGenericMonthForecast(Category category, List<Transaction> history, IEnumerable<Transaction> templates)
        {
            // generic forecast assumes all recurring templates apply
            var recurring = templates.Where(x => x.Category == category).Select(x => x.Amount).Sum();

            if (!history.Any(x => x.Category == category))
                return recurring;

            double forecast;
            switch (category.ForecastType)
            {
                case ForecastType.Exception:
                    forecast = recurring; // no forecast
                    break;
                case ForecastType.Monthly:
                case ForecastType.Daily:
                    var mean = CalculateMonthlyMean(category, history, DateTime.Now);
                    forecast = Math.Abs(recurring) > Math.Abs(mean)
                        ? recurring : mean;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return forecast;
        }

        private static double CalculateActualTotal(Category category, List<Transaction> history, DateTime date)
        {
            return history.Where(t => 
                t.Date >= date.FirstDayOfMonth() &&
                t.Date <= date.LastDayOfMonth() &&
                t.Category == category)
                .Select(x => x.Amount).Sum();
        }

        private static double CalculateMonthlyMean(Category category, List<Transaction> history, DateTime date)
        {
            var startDate = history.Select(x => x.Date).Min();
            var monthTotals = new List<double>();

            var month = date.PreviousMonth().FirstDayOfMonth();
            while (month >= startDate)
            {
                monthTotals.Add(history.Where(t => t.Date >= month && t.Date <= month.LastDayOfMonth() && t.Category == category).Select(t => t.Amount).Sum());
                month = month.PreviousMonth().FirstDayOfMonth();
            }

            return monthTotals.Average();
        }

        private static double CalculateDailyForecast(Category category, List<Transaction> history, DateTime date)
        {
            // calculate daily mean from history
            var startDate = history.Select(x => x.Date).Min();
            var numberOfDays = Math.Ceiling((date - startDate).TotalDays);
            var total = history.Where(t => t.Date <= date && t.Category == category).Select(t => t.Amount).Sum();
            var dailyMean = total / numberOfDays;
            
            // calculate current total
            var currentTotal = history.Where(t => t.Date >= date.FirstDayOfMonth() && t.Date <= date && t.Category == category)
                .Select(x => x.Amount).Sum();

            // extrapolate for remainder of month
            var daysRemaining = Math.Ceiling((date.LastDayOfMonth() - date).TotalDays);

            return currentTotal + daysRemaining * dailyMean;
        }

        

    }
}
