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
    public enum GoalDetailViewMode { Month, Year}

    public class MonthGoalDetailsViewModel : ObservableObject
    {
        private DateTime _month;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private GoalMonthViewModel _selectedGoalMonthViewModel;
        private AsyncRelayCommand _nextMonthCommand;
        private AsyncRelayCommand _previousMonthCommand;
        private string _title;
        private GoalDetailViewMode _viewMode;

        public ObservableCollection<GoalMonthViewModel> GoalsMonthViewModels { get; }

        public string Title
        {
            get => _title;
            set => SetValue(ref _title, value);
        }

        public GoalDetailViewMode ViewMode
        {
            get => _viewMode;
            set => SetValue(ref _viewMode, value);
        }

        public GoalMonthViewModel SelectedGoalMonthViewModel
        {
            get => _selectedGoalMonthViewModel;
            set => SetValue(ref _selectedGoalMonthViewModel, value);
        }

        public MonthGoalDetailsViewModel(DateTime month, IRepository repository, IWindowService windowService)
        {
            Month = month;
            _repository = repository;
            _windowService = windowService;
            GoalsMonthViewModels = new ObservableCollection<GoalMonthViewModel>();
            _repository.GoalsUpdated += async (s, a) => await Initialize();
            _repository.TransactionUpdated += async (s, a) => await Initialize();
            _repository.TransactionDeleted += async (s, a) => await Initialize();
        }

        public async Task Initialize()
        {
            var selectedGoal = SelectedGoalMonthViewModel;

            GoalsMonthViewModels.Clear();

            var goals = await _repository.GetGoals();
            foreach (var goal in goals.OrderBy(x => x.Name))
            {
                var vm = new GoalMonthViewModel(goal, _month, _repository, _windowService);
                await vm.Initialize();
                GoalsMonthViewModels.Add(vm);
            }

            SelectedGoalMonthViewModel = GoalsMonthViewModels.FirstOrDefault(x => x.Name == selectedGoal?.Name) ??
                                         GoalsMonthViewModels.FirstOrDefault();

            Title = string.Format(Captions.TitleMonthGoals, _month.ToString("Y"));
        }

        public DateTime Month
        {
            get => _month;
            set => SetValue(ref _month, value);
        }

        public AsyncRelayCommand NextMonthCommand => _nextMonthCommand ?? (_nextMonthCommand = new AsyncRelayCommand(NextMonth));

        private async Task NextMonth()
        {
            Month = Month.NextMonth();
            await Initialize();
        }

        public AsyncRelayCommand PreviousMonthCommand => _previousMonthCommand ?? (_previousMonthCommand = new AsyncRelayCommand(PreviousMonth));

        private async Task PreviousMonth()
        {
            Month = Month.PreviousMonth();
            await Initialize();
        }
    }
}
