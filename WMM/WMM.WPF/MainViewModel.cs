using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using WMM.Data;
using WMM.WPF.Balances;
using WMM.WPF.Categories;
using WMM.WPF.Forecast;
using WMM.WPF.Helpers;
using WMM.WPF.MVVM;
using WMM.WPF.Recurring;
using WMM.WPF.Resources;
using WMM.WPF.Transactions;

namespace WMM.WPF
{
    public class MainViewModel : ObservableObject
    {
        private readonly IRepository _repository;
        private readonly IWindowService _windowService;
        private AsyncRelayCommand _showRecurringTransactionsCommand;
        private RelayCommand _showManageCategoriesCommand;
        private AsyncRelayCommand _showForecastCommand;

        public MainViewModel(IRepository repository, IWindowService windowService)
        {
            _repository = repository;
            _windowService = windowService;
            _windowService = windowService;
            MonthBalanceViewModels = new ObservableCollection<MonthBalanceViewModel>();

            _repository.TransactionAdded += async (s, a) => { await OnTransactionAddedDeleted(a.Transaction); };
            _repository.TransactionDeleted += async (s, a) => { await OnTransactionAddedDeleted(a.Transaction); };
            _repository.TransactionUpdated += async (s, a) => { await OnTransactionModified(a.OldTransaction, a.NewTransaction); };
            _repository.TransactionBulkUpdated += async (s, a) => { await OnTransactionBulkModified(); };

            AddTransactionsViewModel = new AddTransactionsViewModel(_repository,_windowService);
            AddTransactionsViewModel.UseAsTemplateRequested +=
                (s, a) => AddTransactionsViewModel.UseTransactionAsTemplate(a.Transaction);

            SearchTransactions = new SearchTransactionListViewModel(_repository, _windowService);
            SearchTransactions.UseAsTemplateRequested +=
                (s, a) => AddTransactionsViewModel.UseTransactionAsTemplate(a.Transaction);
        }
        
        public async Task Initialize()
        {
            await _repository.Initialize();
            await AddTransactionsViewModel.Initialize();
            SearchTransactions.Initialize();
            
            var i = 0;
            while(true)
            {
                var monthBalanceViewModel = new MonthBalanceViewModel(DateTime.Now.PreviousMonth(i), _repository, _windowService);
                await monthBalanceViewModel.Initialize();

                if (monthBalanceViewModel.TotalBalance.Income > 0 || monthBalanceViewModel.TotalBalance.Expense < 0 || i == 0) // always add current month
                {
                    monthBalanceViewModel.DetailTransactionsRequested += async (sender, args) =>
                    {
                        await SearchTransactions.SearchForDatesAndCategory(args.DateFrom, args.DateTo, args.Category);
                    };
                    MonthBalanceViewModels.Add(monthBalanceViewModel);
                    i++;
                }
                else
                {
                    break;
                }
            }
            
        }

        public string AppVersion => $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        
        public ObservableCollection<MonthBalanceViewModel> MonthBalanceViewModels { get; }

        public AddTransactionsViewModel AddTransactionsViewModel { get; }

        public RecurringTransactionsViewModel RecurringTransactionsViewModel { get; set; }
        
        public SearchTransactionListViewModel SearchTransactions { get; }

        public AsyncRelayCommand ShowRecurringTransactionsCommand => 
            _showRecurringTransactionsCommand ?? (_showRecurringTransactionsCommand = new AsyncRelayCommand(ShowRecurringTransactions));

        private async Task ShowRecurringTransactions()
        {
            RecurringTransactionsViewModel = new RecurringTransactionsViewModel(_repository, _windowService);
            await RecurringTransactionsViewModel.Initialize();
            _windowService.OpenDialogWindow(RecurringTransactionsViewModel);
        }

        private async Task OnTransactionModified(Transaction transactionOld, Transaction transactionNew)
        {
            var monthViewModelOld = MonthBalanceViewModels.FirstOrDefault(x => x.Month.FirstDayOfMonth() == transactionOld.Date.FirstDayOfMonth());
            var monthViewModelNew = MonthBalanceViewModels.FirstOrDefault(x => x.Month.FirstDayOfMonth() == transactionNew.Date.FirstDayOfMonth());

            if (monthViewModelOld != null)
                await monthViewModelOld.RecalculateBalances(transactionOld.Category);
            if(monthViewModelNew != null)
                await monthViewModelNew.RecalculateBalances(transactionNew.Category);
        }

        private async Task OnTransactionAddedDeleted(Transaction transaction)
        {
            var month = transaction.Date.FirstDayOfMonth();
            var monthViewModel = MonthBalanceViewModels.FirstOrDefault(x => x.Month.FirstDayOfMonth() == month);
            if (monthViewModel != null)
                await monthViewModel.RecalculateBalances(transaction.Category);
        }

        private async Task OnTransactionBulkModified()
        {
            foreach (var monthBalanceViewModel in MonthBalanceViewModels)
            {
                await monthBalanceViewModel.RecalculateBalances();
            }
        }

        public RelayCommand ShowManageCategoriesCommand => _showManageCategoriesCommand ?? (_showManageCategoriesCommand = new RelayCommand(ShowManageCategories));

        private void ShowManageCategories()
        {
            var manageCategoriesViewModel = new ManageCategoriesViewModel(_repository, _windowService);
            manageCategoriesViewModel.Initialize();
            _windowService.OpenDialogWindow(manageCategoriesViewModel);
        }

        public AsyncRelayCommand ShowForecastCommand =>
            _showForecastCommand ?? (_showForecastCommand = new AsyncRelayCommand(ShowForecast));

        private async Task ShowForecast()
        {
            var forecastViewModel = new ForecastViewModel(_repository, _windowService);
            await forecastViewModel.Initialize();
            _windowService.OpenDialogWindow(forecastViewModel);
        }
    }
}
