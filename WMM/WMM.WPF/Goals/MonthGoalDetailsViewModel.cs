using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Resources;

namespace WMM.WPF.Goals
{
    public class MonthGoalDetailsViewModel : ObservableObject
    {
        private readonly DateTime _month;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private GoalMonthViewModel _selectedGoalMonthViewModel;
        
        public ObservableCollection<GoalMonthViewModel> Goals { get; }
        public string Title => string.Format(Captions.TitleMonthGoals, _month.ToString("Y"));

        public GoalMonthViewModel SelectedGoalMonthViewModel
        {
            get => _selectedGoalMonthViewModel;
            set => SetValue(ref _selectedGoalMonthViewModel, value);
        }

        public MonthGoalDetailsViewModel(DateTime month, IRepository repository, IWindowService windowService)
        {
            _month = month;
            _repository = repository;
            _windowService = windowService;
            Goals = new ObservableCollection<GoalMonthViewModel>();
            _repository.GoalsUpdated += async (s, a) => await Initialize();
            _repository.TransactionUpdated += async (s, a) => await Initialize();
            _repository.TransactionDeleted += async (s, a) => await Initialize();
        }

        public async Task Initialize()
        {
            var selectedGoal = SelectedGoalMonthViewModel;

            Goals.Clear();

            var goals = await _repository.GetGoals();
            foreach (var goal in goals.OrderBy(x => x.Name))
            {
                var vm = new GoalMonthViewModel(goal, _month, _repository, _windowService);
                await vm.Initialize();
                Goals.Add(vm);
            }

            SelectedGoalMonthViewModel = Goals.FirstOrDefault(x => x.Name == selectedGoal?.Name) ??
                                         Goals.FirstOrDefault();
        }
        
        
    }
}
