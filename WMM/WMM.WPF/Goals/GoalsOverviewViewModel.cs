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

    public class GoalsOverviewViewModel : ObservableObject
    {
        private DateTime _month;
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private GoalMonthDetailsViewModel _selectedGoalMonthDetailsViewModel;
        private AsyncRelayCommand _nextMonthCommand;
        private AsyncRelayCommand _previousMonthCommand;
        private GoalDetailViewMode _viewMode;
        private GoalYearDetailsViewModel _selectedGoalYearDetailsViewModel;
        private AsyncRelayCommand<DateTime> _goToMonthCommand;

        public ObservableCollection<GoalMonthDetailsViewModel> GoalMonthViewModels { get; }
        public ObservableCollection<GoalYearDetailsViewModel> GoalYearViewModels { get; }

        public GoalDetailViewMode ViewMode
        {
            get => _viewMode;
            set => SetValue(ref _viewMode, value);
        }

        public GoalMonthDetailsViewModel SelectedGoalMonthDetailsViewModel
        {
            get => _selectedGoalMonthDetailsViewModel;
            set => SetValue(ref _selectedGoalMonthDetailsViewModel, value, SelectedGoalMonthChanged);
        }

        private void SelectedGoalMonthChanged()
        {
            SelectedGoalYearDetailsViewModel = GoalYearViewModels.FirstOrDefault(x => x.Name == SelectedGoalMonthDetailsViewModel?.Name) ?? SelectedGoalYearDetailsViewModel;
        }

        public GoalYearDetailsViewModel SelectedGoalYearDetailsViewModel
        {
            get => _selectedGoalYearDetailsViewModel;
            set => SetValue(ref _selectedGoalYearDetailsViewModel, value, SelectedGoalYearChanged);
        }

        private void SelectedGoalYearChanged()
        {
            SelectedGoalMonthDetailsViewModel = GoalMonthViewModels.FirstOrDefault(x => x.Name == SelectedGoalYearDetailsViewModel?.Name) ?? SelectedGoalMonthDetailsViewModel;
        }

        public GoalsOverviewViewModel(DateTime month, IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;

            ViewMode = GoalDetailViewMode.Month;
            Month = month;
            GoalMonthViewModels = new ObservableCollection<GoalMonthDetailsViewModel>();
            GoalYearViewModels = new ObservableCollection<GoalYearDetailsViewModel>();

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
            var selectedGoalName = SelectedGoalMonthDetailsViewModel?.Name;

            GoalMonthViewModels.Clear();

            var goals = await _repository.GetGoals();
            foreach (var goal in goals.OrderBy(x => x.Name))
            {
                var monthViewModel = new GoalMonthDetailsViewModel(goal, _month, _repository, _windowService);
                await monthViewModel.Initialize();
                GoalMonthViewModels.Add(monthViewModel);
            }

            SelectedGoalMonthDetailsViewModel = GoalMonthViewModels.FirstOrDefault(x => x.Name == selectedGoalName) ??
                                         GoalMonthViewModels.FirstOrDefault();
        }

        public async Task InitializeYearViewModels()
        {
            var selectedGoalName = SelectedGoalYearDetailsViewModel?.Name;

            GoalYearViewModels.Clear();

            var goals = await _repository.GetGoals();
            foreach (var goal in goals.OrderBy(x => x.Name))
            {
                var yearViewModel = new GoalYearDetailsViewModel(goal, _repository);
                await yearViewModel.Initialize();
                GoalYearViewModels.Add(yearViewModel);
            }

            SelectedGoalYearDetailsViewModel = GoalYearViewModels.FirstOrDefault(x => x.Name == selectedGoalName) ??
                                               GoalYearViewModels.FirstOrDefault();
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

        public AsyncRelayCommand<DateTime> GoToMonthCommand => _goToMonthCommand ?? (_goToMonthCommand = new AsyncRelayCommand<DateTime>(GoToMonth));

        private async Task GoToMonth(DateTime month)
        {
            Month = month;
            await InitializeMonthViewModels();
            ViewMode = GoalDetailViewMode.Month;
        }
    }
}
