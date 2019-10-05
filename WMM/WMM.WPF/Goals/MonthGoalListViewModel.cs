using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF.Goals
{
    public class MonthGoalListViewModel : ObservableObject
    {
        private readonly DateTime _month;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private GoalMonthViewModel _selectedGoalMonthViewModel;
        private RelayCommand<GoalMonthViewModel> _showGoalMonthDetailsCommand;

        public ObservableCollection<GoalMonthViewModel> Goals { get; }

        public GoalMonthViewModel SelectedGoalMonthViewModel
        {
            get => _selectedGoalMonthViewModel;
            set => SetValue(ref _selectedGoalMonthViewModel, value);
        }

        public MonthGoalListViewModel(DateTime month, IRepository repository, IWindowService windowService)
        {
            _month = month;
            _repository = repository;
            _windowService = windowService;
            Goals = new ObservableCollection<GoalMonthViewModel>();
        }

        public async Task Initialize()
        {
            var goals = await _repository.GetGoals();
            foreach (var goal in goals)
            {
                var vm = new GoalMonthViewModel(goal, _month, _repository);
                await vm.Initialize();
                Goals.Add(vm);
            }

            SelectedGoalMonthViewModel = Goals.FirstOrDefault();
        }

        public RelayCommand<GoalMonthViewModel> ShowGoalMonthDetailsCommand =>
            _showGoalMonthDetailsCommand ??
            (_showGoalMonthDetailsCommand = new RelayCommand<GoalMonthViewModel>(ShowGoalMonthDetails));

        private void ShowGoalMonthDetails(GoalMonthViewModel goalMonthViewModel)
        {
            SelectedGoalMonthViewModel = goalMonthViewModel;
            _windowService.OpenDialogWindow(this);
        }
    }
}
