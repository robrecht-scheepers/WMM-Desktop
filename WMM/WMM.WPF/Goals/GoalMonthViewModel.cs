using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
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

        public double Limit => _goal.Limit;
        public string Name => _goal.Name;
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
        }



    }
}
