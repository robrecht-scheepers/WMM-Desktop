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

            AddTransactionsViewModel = new AddTransactionsViewModel(_repository,_windowService);
            AddTransactionsViewModel.TransactionModified += 
                async (s, a) => { await OnTransactionModified(a.Transaction); };
            AddTransactionsViewModel.UseAsTemplateRequested +=
                (s, a) => AddTransactionsViewModel.UseTransactionAsTemplate(a.Transaction);

            SearchTransactions = new SearchTransactionListViewModel(_repository, _windowService);
            SearchTransactions.TransactionModified +=
                async (s, a) => { await OnTransactionModified(a.Transaction); };
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

        public string AppTitle => $"Haushaltsbuch - {Assembly.GetExecutingAssembly().GetName().Version}";
        
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

        private async Task OnTransactionModified(Transaction transaction)
        {
            var newTransaction = transaction;
            var month = newTransaction.Date.FirstDayOfMonth();
            var monthViewModel = MonthBalanceViewModels.FirstOrDefault(x => x.Month.FirstDayOfMonth() == month);
            if(monthViewModel != null)
                await monthViewModel.RecalculateBalances(newTransaction.Date, newTransaction.Category);
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
            var forecastViewModel = new ForecastViewModel(_repository);
            await forecastViewModel.Initialize();
            _windowService.OpenDialogWindow(forecastViewModel);
        }
    }
}
