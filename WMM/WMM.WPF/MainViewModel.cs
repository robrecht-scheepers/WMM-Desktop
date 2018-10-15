using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;

namespace WMM.WPF
{
    public class MainViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private RelayCommand _showRecurringTransactionsCommand;

        public MainViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            _windowService = windowService;
            MonthBalanceViewModels = new ObservableCollection<MonthBalanceViewModel>();
            AddTransactionsViewModel = new AddTransactionsViewModel(_repository);
            AddTransactionsViewModel.TransactionAdded += (s, a) =>
            {
                var newTransaction = a.Transaction;
                var month = newTransaction.Date.FirstDayOfMonth();
                var monthViewModel = MonthBalanceViewModels.FirstOrDefault(x => x.Month.FirstDayOfMonth() == month);
                // calling without await, because we are in a synchronous event handler
                monthViewModel?.RecalculateForTransaction(newTransaction); 
            };
            RecurringTransactionsViewModel = new RecurringTransactionsViewModel(_repository);
        }

        public async Task Initialize()
        {
            await _repository.Initialize();
            await AddTransactionsViewModel.Initialize();
            await RecurringTransactionsViewModel.Initialize();

            // apply recurring costs or the current month, if not done already
            if (! await _repository.PeriodHasRecurringTransactions(DateTime.Now.FirstDayOfMonth(),
                DateTime.Now.LastDayOfMonth()))
            {
                await _repository.ApplyRecurringTransactions(DateTime.Now.FirstDayOfMonth());
            }

            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now, _repository));
            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now.PreviousMonth(), _repository));
            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now.PreviousMonth().PreviousMonth(), _repository));
            foreach (var monthBalanceViewModel in MonthBalanceViewModels)
            {
                await monthBalanceViewModel.Initialize();
            }
        }

        public ObservableCollection<MonthBalanceViewModel> MonthBalanceViewModels { get; }

        public AddTransactionsViewModel AddTransactionsViewModel { get; }

        public RecurringTransactionsViewModel RecurringTransactionsViewModel { get; }

        public RelayCommand ShowRecurringTransactionsCommand => 
            _showRecurringTransactionsCommand ?? (_showRecurringTransactionsCommand = new RelayCommand(ShowRecurringTransactions));

        private void ShowRecurringTransactions()
        {
            _windowService.OpenDialogWindow(RecurringTransactionsViewModel);
        }
    }
}
