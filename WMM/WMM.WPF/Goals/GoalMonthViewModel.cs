using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WMM.Data;
using WMM.WPF.Forecast;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF.Goals
{
    public class GoalMonthViewModel : ObservableObject
    {
        private readonly DateTime _month;
        private readonly IRepository _repository;
        private readonly Goal _goal;
        private double _currentAmount;
        private GoalStatus _status;
        private List<DateAmountSeries> _chartSeries;

        public double Limit => _goal.Limit;
        public string Name => _goal.Name;

        public string Description => _goal.Description;

        public string CriteriaString => CreateCriteriaString(_goal);

        public List<Transaction> Transactions { get; private set; }

        public double CurrentAmount
        {
            get => _currentAmount;
            set => SetValue(ref _currentAmount, value);
        }

        public GoalStatus Status
        {
            get => _status;
            set => SetValue(ref _status, value);
        }

        public List<DateAmountSeries> ChartSeries
        {
            get => _chartSeries;
            set => SetValue(ref _chartSeries, value);
        }

        public GoalMonthViewModel(Goal goal, DateTime month, IRepository repository)
        {
            _month = month;
            _repository = repository;
            _goal = goal;
        }

        public async Task Initialize()
        {
            Transactions = (await _repository.GetTransactions(_month.FirstDayOfMonth(), _month.LastDayOfMonth(), _goal)).ToList();
            var info = GoalCalculator.CalculateGoalMonthInfo(_goal, _month, Transactions);

            CurrentAmount = info.CurrentAmount;
            Status = info.Status;
            ChartSeries = new List<DateAmountSeries>
            {
                new DateAmountSeries
                {
                    Points = info.IdealPoints,
                    Brush = Brushes.DarkGreen
                },
                new DateAmountSeries
                {
                    Points = info.ActualPoints,
                    Brush = Brushes.DodgerBlue
                }
            };
        }

        private string CreateCriteriaString(Goal goal)
        {
            var stringBuilder = new StringBuilder();
            var first = true;
            foreach (var categoryType in goal.CategoryTypeCriteria)
            {
                if (!first)
                    stringBuilder.Append(", ");
                stringBuilder.Append(categoryType.ToCaption());
                first = false;
            }

            foreach (var area in goal.AreaCriteria)
            {
                if (!first)
                    stringBuilder.Append(", ");
                stringBuilder.Append(area);
                first = false;
            }

            foreach (var category in goal.CategoryCriteria)
            {
                if (!first)
                    stringBuilder.Append(", ");
                stringBuilder.Append(category.Name);
                first = false;
            }

            return stringBuilder.ToString();
        }



    }
}
