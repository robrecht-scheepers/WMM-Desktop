using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.MVVM;

namespace WMM.WPF.Goals
{
    public class GoalMonthViewModel : ObservableObject
    {
        private readonly DateTime _month;
        private double _currentAmount;
        public Goal Goal { get; }
        public double Limit => Goal.Limit;
        public string Name => Goal.Name;

        public double CurrentAmount
        {
            get => _currentAmount;
            set => SetValue(ref _currentAmount, value);
        }

        public GoalMonthViewModel(Goal goal, DateTime month)
        {
            _month = month;
            Goal = goal;
        }

    }
}
