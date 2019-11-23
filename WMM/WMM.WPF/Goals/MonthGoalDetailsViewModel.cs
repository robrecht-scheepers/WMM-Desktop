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
        private GoalYearViewModel _selectedGoalYearViewModel;

        public ObservableCollection<GoalMonthViewModel> GoalMonthViewModels { get; }
        public ObservableCollection<GoalYearViewModel> GoalYearViewModels { get; }

        public GoalDetailViewMode ViewMode
        {
            get => _viewMode;
            set => SetValue(ref _viewMode, value);
        }

        public GoalMonthViewModel SelectedGoalMonthViewModel
        {
            get => _selectedGoalMonthViewModel;
            set => SetValue(ref _selectedGoalMonthViewModel, value, SelectedGoalMonthChanged);
        }

        private void SelectedGoalMonthChanged()
        {
            SelectedGoalYearViewModel = GoalYearViewModels.FirstOrDefault(x => x.Name == SelectedGoalMonthViewModel?.Name) ?? SelectedGoalYearViewModel;
        }

        public GoalYearViewModel SelectedGoalYearViewModel
        {
            get => _selectedGoalYearViewModel;
            set => SetValue(ref _selectedGoalYearViewModel, value, SelectedGoalYearChanged);
        }

        private void SelectedGoalYearChanged()
        {
            SelectedGoalMonthViewModel = GoalMonthViewModels.FirstOrDefault(x => x.Name == SelectedGoalYearViewModel?.Name) ?? SelectedGoalMonthViewModel;
        }

        public MonthGoalDetailsViewModel(DateTime month, IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;

            ViewMode = GoalDetailViewMode.Month;
            Month = month;
            GoalMonthViewModels = new ObservableCollection<GoalMonthViewModel>();
            GoalYearViewModels = new ObservableCollection<GoalYearViewModel>();

            _repository.GoalsUpdated += async (s, a) => await Initialize();
            _repository.TransactionUpdated += async (s, a) => await Initialize();
            _repository.TransactionDeleted += async (s, a) => await Initialize();
        }

        public async Task Initialize()
        {
            await InitializeMonthViewModels();
            await InitializeYearViewModels();
        }

        public async Task InitializeMonthViewModels()
        {
            var selectedGoal = SelectedGoalMonthViewModel;

            GoalMonthViewModels.Clear();

            var goals = await _repository.GetGoals();
            foreach (var goal in goals.OrderBy(x => x.Name))
            {
                var monthViewModel = new GoalMonthViewModel(goal, _month, _repository, _windowService);
                await monthViewModel.Initialize();
                GoalMonthViewModels.Add(monthViewModel);
            }

            SelectedGoalMonthViewModel = GoalMonthViewModels.FirstOrDefault(x => x.Name == selectedGoal?.Name) ??
                                         GoalMonthViewModels.FirstOrDefault();
        }

        public async Task InitializeYearViewModels()
        {
            GoalYearViewModels.Clear();

            var goals = await _repository.GetGoals();
            foreach (var goal in goals.OrderBy(x => x.Name))
            {
                var yearViewModel = new GoalYearViewModel(goal, _repository);
                await yearViewModel.Initialize();
                GoalYearViewModels.Add(yearViewModel);
            }
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
            await InitializeMonthViewModels();
        }

        public AsyncRelayCommand PreviousMonthCommand => _previousMonthCommand ?? (_previousMonthCommand = new AsyncRelayCommand(PreviousMonth));

        private async Task PreviousMonth()
        {
            Month = Month.PreviousMonth();
            await InitializeMonthViewModels();
        }
    }
}
