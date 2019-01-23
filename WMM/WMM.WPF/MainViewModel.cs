using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WMM.Data;
using WMM.WPF.Balances;
using WMM.WPF.Categories;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Recurring;
using WMM.WPF.Transactions;

namespace WMM.WPF
{
    public class MainViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private RelayCommand _showRecurringTransactionsCommand;
        private AsyncRelayCommand _showManageCategoriesCommand;

        public MainViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            _windowService = windowService;
            MonthBalanceViewModels = new ObservableCollection<MonthBalanceViewModel>();

            AddTransactionsViewModel = new AddTransactionsViewModel(_repository,_windowService);
            AddTransactionsViewModel.TransactionModified += 
                async (s, a) => { await OnTransactionModified(a.Transaction); };

            DetailTransactions = new DetailTransactionListViewModel(_repository, _windowService);
            DetailTransactions.TransactionModified += 
                async (s, a) => { await OnTransactionModified(a.Transaction); };

            SearchTransactions = new SearchTransactionListViewModel(_repository, _windowService);
            SearchTransactions.TransactionModified +=
                async (s, a) => { await OnTransactionModified(a.Transaction); };

            RecurringTransactionsViewModel = new RecurringTransactionsViewModel(_repository, _windowService);
        }
        
        public async Task Initialize()
        {
            await _repository.Initialize();
            await AddTransactionsViewModel.Initialize();
            await RecurringTransactionsViewModel.Initialize();
            SearchTransactions.Initialize();

            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now, _repository, _windowService));
            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now.PreviousMonth(), _repository, _windowService));
            MonthBalanceViewModels.Add(new MonthBalanceViewModel(DateTime.Now.PreviousMonth().PreviousMonth(), _repository, _windowService));
            foreach (var monthBalanceViewModel in MonthBalanceViewModels)
            {
                await monthBalanceViewModel.Initialize();
                monthBalanceViewModel.DetailTransactionsRequested += async (sender, args) =>
                {
                    await DetailTransactions.LoadTransactions(args.DateFrom, args.DateTo, args.Category);
                };
            }
        }
        
        public ObservableCollection<MonthBalanceViewModel> MonthBalanceViewModels { get; }

        public AddTransactionsViewModel AddTransactionsViewModel { get; }

        public RecurringTransactionsViewModel RecurringTransactionsViewModel { get; }

        public DetailTransactionListViewModel DetailTransactions { get; }

        public SearchTransactionListViewModel SearchTransactions { get; }

        public RelayCommand ShowRecurringTransactionsCommand => 
            _showRecurringTransactionsCommand ?? (_showRecurringTransactionsCommand = new RelayCommand(ShowRecurringTransactions));

        private void ShowRecurringTransactions()
        {
            _windowService.OpenDialogWindow(RecurringTransactionsViewModel);
        }

        private async Task OnTransactionModified(Transaction transaction)
        {
            var newTransaction = transaction;
            var month = newTransaction.Date.FirstDayOfMonth();
            var monthViewModel = MonthBalanceViewModels.FirstOrDefault(x => x.Month.FirstDayOfMonth() == month);
            if(monthViewModel != null)
                await monthViewModel.RecalculateBalances(newTransaction.Date, newTransaction.Category);
            await DetailTransactions.ReloadTransactions();
        }

        public AsyncRelayCommand ShowManageCategoriesCommand => _showManageCategoriesCommand ?? (_showManageCategoriesCommand = new AsyncRelayCommand(ShowManageCategories));

        private async Task ShowManageCategories()
        {
            var manageCategoriesViewModel = new ManageCategoriesViewModel(_repository, _windowService);
            await manageCategoriesViewModel.Initialize();
            _windowService.OpenDialogWindow(manageCategoriesViewModel);
        }
    }
}
